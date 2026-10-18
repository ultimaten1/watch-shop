using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WatchShop.Api.Models;
using WatchShop.Api.Options;

namespace WatchShop.Api.Services;

public class TokenService(IOptions<JwtOptions> options)
{
    private readonly JwtOptions _jwt = options.Value;

    /// <summary>Admin có role "Admin"; chủ cửa hàng có thêm role "Owner".</summary>
    public (string Token, DateTime Expires) ForAdmin(AdminUser u)
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

    public (string Token, DateTime Expires) ForCustomer(Customer c) => Create(new[]
    {
        new Claim(ClaimTypes.NameIdentifier, c.Id.ToString()),
        new Claim(ClaimTypes.Name, c.Email),
        new Claim(ClaimTypes.Role, "Customer")
    });

    private (string, DateTime) Create(IEnumerable<Claim> claims)
    {
        var expires = DateTime.UtcNow.AddHours(_jwt.ExpireHours);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var token = new JwtSecurityToken(_jwt.Issuer, _jwt.Audience, claims, expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
