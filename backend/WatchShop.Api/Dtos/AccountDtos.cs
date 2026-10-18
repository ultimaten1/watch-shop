using System.ComponentModel.DataAnnotations;

namespace WatchShop.Api.Dtos;

// ---------- Tài khoản quản trị ----------
public record AdminMeDto(string Username, string FullName, string Role);

public record AdminUserDto(int Id, string Username, string FullName, string Role, bool IsActive);

public class AdminUserCreateDto
{
    [Required, StringLength(50, MinimumLength = 3), RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Tên đăng nhập chỉ gồm chữ không dấu, số, . _ -")]
    public string Username { get; set; } = "";
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    [Required, StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự")]
    public string Password { get; set; } = "";
    public string Role { get; set; } = "Staff";
}

public class AdminUserUpdateDto
{
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    public string Role { get; set; } = "Staff";
    public bool IsActive { get; set; } = true;
    /// <summary>Để trống nếu không đặt lại mật khẩu</summary>
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự")]
    public string? NewPassword { get; set; }
}

public class ChangePasswordDto
{
    [Required] public string CurrentPassword { get; set; } = "";
    [Required, StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu mới tối thiểu 8 ký tự")]
    public string NewPassword { get; set; } = "";
}

// ---------- Tài khoản khách ----------
public class CustomerRegisterDto
{
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = "";
    [Required, StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự")]
    public string Password { get; set; } = "";
    [RegularExpression(@"^(0|\+84)\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? Phone { get; set; }
}

public class CustomerLoginDto
{
    [Required] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}

public class CustomerProfileDto
{
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    [RegularExpression(@"^(0|\+84)\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? Phone { get; set; }
    [StringLength(300)] public string? Address { get; set; }
}

public record CustomerDto(int Id, string FullName, string Email, string? Phone, string? Address);

public record CustomerAuthResult(string Token, DateTime ExpiresAt, CustomerDto Customer);
