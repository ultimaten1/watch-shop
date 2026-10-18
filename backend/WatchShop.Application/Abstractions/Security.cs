using WatchShop.Domain.Entities;

namespace WatchShop.Application.Abstractions;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public record TokenResult(string Token, DateTime ExpiresAt);

public interface ITokenService
{
    /// <summary>Admin có role "Admin"; chủ cửa hàng có thêm role "Owner".</summary>
    TokenResult ForAdmin(AdminUser user);
    TokenResult ForCustomer(Customer customer);
}
