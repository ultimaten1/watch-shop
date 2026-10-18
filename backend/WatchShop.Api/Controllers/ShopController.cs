using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WatchShop.Api.Data;
using WatchShop.Api.Models;
using WatchShop.Api.Options;

namespace WatchShop.Api.Controllers;

[ApiController]
public class ShopController(AppDbContext db, IOptions<ShopOptions> shop) : ControllerBase
{
    /// <summary>Thông tin cửa hàng hiển thị trên website (lấy từ appsettings "Shop").</summary>
    [HttpGet("api/shop")]
    public ActionResult<ShopOptions> Info() => shop.Value;

    /// <summary>Số liệu nhanh cho trang quản trị.</summary>
    [HttpGet("api/admin/stats")]
    [Authorize]
    public async Task<ActionResult<object>> Stats() => new
    {
        newOrders = await db.Orders.CountAsync(o => o.Status == OrderStatus.New),
        unreadMessages = await db.ContactMessages.CountAsync(m => !m.IsRead),
        products = await db.Products.CountAsync(),
        lowStock = await db.Products.CountAsync(p => p.Stock <= 2 && p.IsActive)
    };
}
