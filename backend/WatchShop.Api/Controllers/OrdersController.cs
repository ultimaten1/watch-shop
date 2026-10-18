using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WatchShop.Api.Data;
using WatchShop.Api.Dtos;
using WatchShop.Api.Models;
using WatchShop.Api.Options;

namespace WatchShop.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(AppDbContext db, IOptions<PaymentOptions> paymentOptions) : ControllerBase
{
    /// <summary>Khách đặt hàng. Giá và tồn kho luôn được tính lại ở server.</summary>
    [HttpPost]
    [EnableRateLimiting("forms")]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderDto dto)
    {
        // Gộp các dòng trùng sản phẩm
        var wanted = dto.Items
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

        var ids = wanted.Keys.ToList();
        var products = await db.Products.Where(p => ids.Contains(p.Id) && p.IsActive).ToListAsync();
        if (products.Count != ids.Count)
            return BadRequest(new { error = "Có sản phẩm không còn bán, vui lòng kiểm tra lại giỏ hàng" });

        foreach (var p in products)
        {
            if (wanted[p.Id] > p.Stock)
                return BadRequest(new { error = $"\"{p.Name}\" chỉ còn {p.Stock} chiếc" });
        }

        var order = new Order
        {
            Code = await GenerateCodeAsync(),
            CustomerName = dto.CustomerName.Trim(),
            Phone = dto.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
            Address = dto.Address.Trim(),
            Note = dto.Note,
            PaymentMethod = dto.PaymentMethod,
            Status = OrderStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var p in products)
        {
            var qty = wanted[p.Id];
            order.Items.Add(new OrderItem
            {
                ProductId = p.Id,
                ProductName = p.Name,
                UnitPrice = p.Price,
                Quantity = qty
            });
            p.Stock -= qty;
        }
        order.Total = order.Items.Sum(i => i.UnitPrice * i.Quantity);

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return ToDto(order, includePayment: true);
    }

    /// <summary>Khách tra cứu đơn bằng mã đơn + số điện thoại.</summary>
    [HttpGet("lookup")]
    [EnableRateLimiting("forms")]
    public async Task<ActionResult<OrderDto>> Lookup([FromQuery] string code, [FromQuery] string phone)
    {
        var c = (code ?? "").Trim().ToUpperInvariant();
        var ph = (phone ?? "").Trim();
        var order = await db.Orders.AsNoTracking().Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Code == c && o.Phone == ph);
        if (order is null) return NotFound(new { error = "Không tìm thấy đơn hàng. Kiểm tra lại mã đơn và số điện thoại." });
        return ToDto(order, includePayment: order.Status == OrderStatus.New);
    }

    // ---------- Quản trị ----------
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<OrderDto>>> List([FromQuery] OrderStatus? status)
    {
        var q = db.Orders.AsNoTracking().Include(o => o.Items).AsQueryable();
        if (status.HasValue) q = q.Where(o => o.Status == status.Value);
        var orders = await q.OrderByDescending(o => o.CreatedAt).Take(200).ToListAsync();
        return orders.Select(o => ToDto(o, false)).ToList();
    }

    [HttpPut("{id:int}/status")]
    [Authorize]
    public async Task<ActionResult<OrderDto>> UpdateStatus(int id, UpdateOrderStatusDto dto)
    {
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();

        if (order.Status == OrderStatus.Cancelled && dto.Status != OrderStatus.Cancelled)
            return BadRequest(new { error = "Đơn đã huỷ không thể mở lại. Hãy tạo đơn mới." });

        // Huỷ đơn: trả hàng về kho
        if (dto.Status == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
        {
            var ids = order.Items.Select(i => i.ProductId).ToList();
            var products = await db.Products.Where(p => ids.Contains(p.Id)).ToListAsync();
            foreach (var item in order.Items)
            {
                var p = products.FirstOrDefault(x => x.Id == item.ProductId);
                if (p is not null) p.Stock += item.Quantity;
            }
        }

        order.Status = dto.Status;
        await db.SaveChangesAsync();
        return ToDto(order, false);
    }

    // ---------- Helpers ----------
    private async Task<string> GenerateCodeAsync()
    {
        while (true)
        {
            var code = $"DH{DateTime.UtcNow:yyMMdd}{Random.Shared.Next(100, 1000)}";
            if (!await db.Orders.AnyAsync(o => o.Code == code)) return code;
        }
    }

    private OrderDto ToDto(Order o, bool includePayment)
    {
        PaymentInfoDto? payment = null;
        if (includePayment && o.PaymentMethod == PaymentMethod.BankTransfer)
        {
            var pay = paymentOptions.Value;
            // Ảnh QR VietQR dựng sẵn: điền sẵn số tiền và nội dung chuyển khoản
            var qr = $"https://img.vietqr.io/image/{pay.BankId}-{pay.AccountNumber}-compact2.png" +
                     $"?amount={(long)o.Total}&addInfo={Uri.EscapeDataString(o.Code)}" +
                     $"&accountName={Uri.EscapeDataString(pay.AccountName)}";
            payment = new PaymentInfoDto(pay.BankName, pay.AccountNumber, pay.AccountName, o.Code, qr);
        }

        return new OrderDto(
            o.Id, o.Code, o.CustomerName, o.Phone, o.Email, o.Address, o.Note,
            o.PaymentMethod, o.Status, o.Total, o.CreatedAt,
            o.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList(),
            payment);
    }
}
