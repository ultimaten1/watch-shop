using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WatchShop.Api.Data;
using WatchShop.Api.Dtos;
using WatchShop.Api.Options;

namespace WatchShop.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, IOptions<JwtOptions> jwtOptions) : ControllerBase
{
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<LoginResult>> Login(LoginDto dto)
    {
        var user = await db.AdminUsers.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { error = "Sai tên đăng nhập hoặc mật khẩu" });

        var jwt = jwtOptions.Value;
        var expires = DateTime.UtcNow.AddHours(jwt.ExpireHours);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new LoginResult(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<object> Me() => new { username = User.Identity?.Name };
}
