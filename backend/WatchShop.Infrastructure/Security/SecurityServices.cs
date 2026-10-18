using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Configuration;
using WatchShop.Domain.Entities;

namespace WatchShop.Infrastructure.Security;

/// <summary>Băm một chiều bằng BCrypt (không thể giải ngược ra mật khẩu).</summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}

public class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    private readonly JwtOptions _jwt = options.Value;

    public TokenResult ForAdmin(AdminUser u)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, u.Id.ToString()),
            new(ClaimTypes.Name, u.Username),
            new(ClaimTypes.Role, "Admin")
        };
        if (u.Role == AdminRoles.Owner) claims.Add(new Claim(ClaimTypes.Role, AdminRoles.Owner));
        return Create(claims);
    }

    public TokenResult ForCustomer(Customer c) => Create(new[]
    {
        new Claim(ClaimTypes.NameIdentifier, c.Id.ToString()),
        new Claim(ClaimTypes.Name, c.Email),
        new Claim(ClaimTypes.Role, "Customer")
    });

    private TokenResult Create(IEnumerable<Claim> claims)
    {
        var expires = DateTime.UtcNow.AddHours(_jwt.ExpireHours);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var token = new JwtSecurityToken(_jwt.Issuer, _jwt.Audience, claims, expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new TokenResult(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
