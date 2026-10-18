using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using WatchShop.Api.Data;
using WatchShop.Api.Dtos;
using WatchShop.Api.Services;

namespace WatchShop.Api.Controllers;

/// <summary>Đăng nhập tài khoản quản trị (chủ cửa hàng và nhân viên).</summary>
[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, TokenService tokens) : ControllerBase
{
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<LoginResult>> Login(LoginDto dto)
    {
        var user = await db.AdminUsers.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user is null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { error = "Sai tên đăng nhập hoặc mật khẩu" });

        var (token, expires) = tokens.ForAdmin(user);
        return new LoginResult(token, expires);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AdminMeDto>> Me()
    {
        var user = await CurrentAsync();
        if (user is null) return Unauthorized();
        return new AdminMeDto(user.Username, user.FullName, user.Role);
    }

    /// <summary>Quản trị viên tự đổi mật khẩu của mình.</summary>
    [HttpPut("password")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var user = await CurrentAsync();
        if (user is null) return Unauthorized();
        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            return BadRequest(new { error = "Mật khẩu hiện tại không đúng" });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<Models.AdminUser?> CurrentAsync()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return null;
        var user = await db.AdminUsers.FindAsync(id);
        return user is { IsActive: true } ? user : null;
    }
}
