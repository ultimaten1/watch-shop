namespace WatchShop.Domain.Entities;

public static class AdminRoles
{
    public const string Owner = "Owner";
    public const string Staff = "Staff";
}

public class AdminUser
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string FullName { get; set; } = "";
    /// <summary>Owner | Staff</summary>
    public string Role { get; set; } = AdminRoles.Staff;
    public bool IsActive { get; set; } = true;
}

public class Customer
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
