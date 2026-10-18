using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using WatchShop.Api.Data;
using WatchShop.Api.Dtos;
using WatchShop.Api.Models;
using WatchShop.Api.Services;

namespace WatchShop.Api.Controllers;

/// <summary>Tài khoản của khách mua hàng.</summary>
[ApiController]
[Route("api/account")]
public class AccountController(AppDbContext db, TokenService tokens) : ControllerBase
{
    private static CustomerDto ToDto(Customer c) => new(c.Id, c.FullName, c.Email, c.Phone, c.Address);
    private static string NormalizeEmail(string e) => e.Trim().ToLowerInvariant();

    [HttpPost("register")]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<CustomerAuthResult>> Register(CustomerRegisterDto dto)
    {
        var email = NormalizeEmail(dto.Email);
        if (await db.Customers.AnyAsync(c => c.Email == email))
            return BadRequest(new { error = "Email này đã được đăng ký. Hãy đăng nhập." });

        var customer = new Customer
        {
            FullName = dto.FullName.Trim(),
            Email = email,
            Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var (token, expires) = tokens.ForCustomer(customer);
        return new CustomerAuthResult(token, expires, ToDto(customer));
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<CustomerAuthResult>> Login(CustomerLoginDto dto)
    {
        var email = NormalizeEmail(dto.Email);
        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Email == email);
        if (customer is null || !BCrypt.Net.BCrypt.Verify(dto.Password, customer.PasswordHash))
            return Unauthorized(new { error = "Sai email hoặc mật khẩu" });

        var (token, expires) = tokens.ForCustomer(customer);
        return new CustomerAuthResult(token, expires, ToDto(customer));
    }

    [HttpGet("me")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<CustomerDto>> Me()
    {
        var c = await CurrentAsync();
        return c is null ? Unauthorized() : ToDto(c);
    }

    [HttpPut("me")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<CustomerDto>> UpdateProfile(CustomerProfileDto dto)
    {
        var c = await CurrentAsync();
        if (c is null) return Unauthorized();
        c.FullName = dto.FullName.Trim();
        c.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        c.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();
        await db.SaveChangesAsync();
        return ToDto(c);
    }

    [HttpPut("password")]
    [Authorize(Roles = "Customer")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var c = await CurrentAsync();
        if (c is null) return Unauthorized();
        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, c.PasswordHash))
            return BadRequest(new { error = "Mật khẩu hiện tại không đúng" });
        c.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<Customer?> CurrentAsync() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? await db.Customers.FindAsync(id) : null;
}
