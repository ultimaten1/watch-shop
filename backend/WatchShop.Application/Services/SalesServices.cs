using Microsoft.Extensions.Options;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Common;
using WatchShop.Application.Configuration;
using WatchShop.Application.Dtos;
using WatchShop.Domain.Entities;
using WatchShop.Domain.Enums;

namespace WatchShop.Application.Services;

public interface IOrderService
{
    /// <param name="customerId">Id khách đã đăng nhập, null nếu khách vãng lai.</param>
    Task<OrderDto> CreateAsync(CreateOrderDto dto, int? customerId, CancellationToken ct = default);
    Task<OrderDto> LookupAsync(string? code, string? phone, CancellationToken ct = default);
    Task<List<OrderDto>> ListForCustomerAsync(int customerId, CancellationToken ct = default);
    Task<List<OrderDto>> ListAsync(OrderStatus? status, CancellationToken ct = default);
    Task<OrderDto> UpdateStatusAsync(int id, OrderStatus status, CancellationToken ct = default);
}

public class OrderService(IUnitOfWork uow, IOptions<PaymentOptions> paymentOptions) : IOrderService
{
    /// <summary>Giá và tồn kho luôn được tính lại ở server.</summary>
    public async Task<OrderDto> CreateAsync(CreateOrderDto dto, int? customerId, CancellationToken ct = default)
    {
        // Gộp các dòng trùng sản phẩm
        var wanted = dto.Items
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

        var ids = wanted.Keys.ToList();
        var products = await uow.Products.GetActiveByIdsAsync(ids, ct);
        if (products.Count != ids.Count)
            throw new BadRequestException("Có sản phẩm không còn bán, vui lòng kiểm tra lại giỏ hàng");

        foreach (var p in products)
        {
            if (wanted[p.Id] > p.Stock)
                throw new BadRequestException($"\"{p.Name}\" chỉ còn {p.Stock} chiếc");
        }

        // Chỉ gắn đơn vào tài khoản nếu tài khoản còn tồn tại
        if (customerId is not null && !await uow.Customers.ExistsAsync(customerId.Value, ct))
            customerId = null;

        var order = new Order
        {
            Code = await GenerateCodeAsync(ct),
            CustomerName = dto.CustomerName.Trim(),
            Phone = dto.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
            Address = dto.Address.Trim(),
            Note = dto.Note,
            PaymentMethod = dto.PaymentMethod,
            Status = OrderStatus.New,
            CreatedAt = DateTime.UtcNow,
            CustomerId = customerId
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

        // Đơn và trừ kho được lưu cùng một lần (một transaction)
        await uow.Orders.AddAsync(order, ct);
        await uow.SaveChangesAsync(ct);

        return ToDto(order, includePayment: true);
    }

    public async Task<OrderDto> LookupAsync(string? code, string? phone, CancellationToken ct = default)
    {
        var c = (code ?? "").Trim().ToUpperInvariant();
        var ph = (phone ?? "").Trim();
        var order = await uow.Orders.FindByCodeAndPhoneAsync(c, ph, ct)
                    ?? throw new NotFoundException("Không tìm thấy đơn hàng. Kiểm tra lại mã đơn và số điện thoại.");
        return ToDto(order, includePayment: order.Status == OrderStatus.New);
    }

    public async Task<List<OrderDto>> ListForCustomerAsync(int customerId, CancellationToken ct = default) =>
        (await uow.Orders.ListByCustomerAsync(customerId, 100, ct))
        .Select(o => ToDto(o, includePayment: o.Status == OrderStatus.New)).ToList();

    public async Task<List<OrderDto>> ListAsync(OrderStatus? status, CancellationToken ct = default) =>
        (await uow.Orders.ListAsync(status, 200, ct)).Select(o => ToDto(o, false)).ToList();

    public async Task<OrderDto> UpdateStatusAsync(int id, OrderStatus status, CancellationToken ct = default)
    {
        var order = await uow.Orders.GetWithItemsAsync(id, ct) ?? throw new NotFoundException();

        if (order.Status == OrderStatus.Cancelled && status != OrderStatus.Cancelled)
            throw new BadRequestException("Đơn đã huỷ không thể mở lại. Hãy tạo đơn mới.");

        // Huỷ đơn: trả hàng về kho
        if (status == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
        {
            var ids = order.Items.Select(i => i.ProductId).ToList();
            var products = await uow.Products.GetByIdsAsync(ids, ct);
            foreach (var item in order.Items)
            {
                var p = products.FirstOrDefault(x => x.Id == item.ProductId);
                if (p is not null) p.Stock += item.Quantity;
            }
        }

        order.Status = status;
        await uow.SaveChangesAsync(ct);
        return ToDto(order, false);
    }

    private async Task<string> GenerateCodeAsync(CancellationToken ct)
    {
        while (true)
        {
            var code = $"DH{DateTime.UtcNow:yyMMdd}{Random.Shared.Next(100, 1000)}";
            if (!await uow.Orders.CodeExistsAsync(code, ct)) return code;
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

public interface IContactService
{
    Task SendAsync(ContactCreateDto dto, CancellationToken ct = default);
    Task<List<ContactMessageDto>> ListAsync(CancellationToken ct = default);
    Task MarkReadAsync(int id, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public class ContactService(IUnitOfWork uow) : IContactService
{
    public async Task SendAsync(ContactCreateDto dto, CancellationToken ct = default)
    {
        await uow.ContactMessages.AddAsync(new ContactMessage
        {
            Name = dto.Name.Trim(),
            Phone = dto.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
            Message = dto.Message.Trim(),
            CreatedAt = DateTime.UtcNow
        }, ct);
        await uow.SaveChangesAsync(ct);
    }

    public async Task<List<ContactMessageDto>> ListAsync(CancellationToken ct = default) =>
        (await uow.ContactMessages.ListRecentAsync(200, ct))
        .Select(m => new ContactMessageDto(m.Id, m.Name, m.Phone, m.Email, m.Message, m.CreatedAt, m.IsRead)).ToList();

    public async Task MarkReadAsync(int id, CancellationToken ct = default)
    {
        var m = await uow.ContactMessages.GetByIdAsync(id, ct) ?? throw new NotFoundException();
        m.IsRead = true;
        await uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var m = await uow.ContactMessages.GetByIdAsync(id, ct) ?? throw new NotFoundException();
        uow.ContactMessages.Remove(m);
        await uow.SaveChangesAsync(ct);
    }
}

public interface IStatsService
{
    Task<AdminStatsDto> GetAsync(CancellationToken ct = default);
}

public class StatsService(IUnitOfWork uow) : IStatsService
{
    public async Task<AdminStatsDto> GetAsync(CancellationToken ct = default) => new(
        await uow.Orders.CountByStatusAsync(OrderStatus.New, ct),
        await uow.ContactMessages.CountUnreadAsync(ct),
        await uow.Products.CountAsync(ct),
        await uow.Products.CountLowStockAsync(2, ct));
}
