using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using WatchShop.Api.Common;
using WatchShop.Application.Configuration;
using WatchShop.Application.Dtos;
using WatchShop.Application.Services;
using WatchShop.Domain.Enums;

namespace WatchShop.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(IOrderService orders) : ControllerBase
{
    /// <summary>Khách đặt hàng. Nếu đã đăng nhập thì đơn được gắn vào tài khoản.</summary>
    [HttpPost]
    [EnableRateLimiting("forms")]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderDto dto, CancellationToken ct)
    {
        int? customerId = User.IsInRole("Customer") ? User.UserId() : null;
        return await orders.CreateAsync(dto, customerId, ct);
    }

    /// <summary>Khách tra cứu đơn bằng mã đơn + số điện thoại.</summary>
    [HttpGet("lookup")]
    [EnableRateLimiting("forms")]
    public async Task<ActionResult<OrderDto>> Lookup([FromQuery] string code, [FromQuery] string phone, CancellationToken ct) =>
        await orders.LookupAsync(code, phone, ct);

    /// <summary>Khách đã đăng nhập xem lịch sử đơn của mình.</summary>
    [HttpGet("mine")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<List<OrderDto>>> Mine(CancellationToken ct) =>
        await orders.ListForCustomerAsync(User.UserId(), ct);

    // ---------- Quản trị ----------
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<OrderDto>>> List([FromQuery] OrderStatus? status, CancellationToken ct) =>
        await orders.ListAsync(status, ct);

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(int id, UpdateOrderStatusDto dto, CancellationToken ct) =>
        await orders.UpdateStatusAsync(id, dto.Status, ct);
}

[ApiController]
[Route("api/contact")]
public class ContactController(IContactService contact) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("forms")]
    public async Task<IActionResult> Send(ContactCreateDto dto, CancellationToken ct)
    {
        await contact.SendAsync(dto, ct);
        return Ok(new { ok = true });
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<ContactMessageDto>>> List(CancellationToken ct) =>
        await contact.ListAsync(ct);

    [HttpPut("{id:int}/read")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MarkRead(int id, CancellationToken ct)
    {
        await contact.MarkReadAsync(id, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await contact.DeleteAsync(id, ct);
        return NoContent();
    }
}

[ApiController]
public class ShopController(IStatsService stats, IOptions<ShopOptions> shop) : ControllerBase
{
    /// <summary>Thông tin cửa hàng hiển thị trên website (lấy từ cấu hình "Shop").</summary>
    [HttpGet("api/shop")]
    public ActionResult<ShopOptions> Info() => shop.Value;

    /// <summary>Số liệu nhanh cho trang quản trị.</summary>
    [HttpGet("api/admin/stats")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AdminStatsDto>> Stats(CancellationToken ct) => await stats.GetAsync(ct);
}
