using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WatchShop.Api.Common;
using WatchShop.Application.Dtos;
using WatchShop.Application.Services;

namespace WatchShop.Api.Controllers;

/// <summary>Đăng nhập tài khoản quản trị (chủ cửa hàng và nhân viên).</summary>
[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<LoginResult>> Login(LoginDto dto, CancellationToken ct) =>
        await auth.LoginAsync(dto, ct);

    [HttpGet("me")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AdminMeDto>> Me(CancellationToken ct) =>
        await auth.MeAsync(User.UserId(), ct);

    /// <summary>Quản trị viên tự đổi mật khẩu của mình.</summary>
    [HttpPut("password")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto, CancellationToken ct)
    {
        await auth.ChangePasswordAsync(User.UserId(), dto, ct);
        return NoContent();
    }
}

/// <summary>Quản lý tài khoản nhân viên. Chỉ chủ cửa hàng (Owner) dùng được.</summary>
[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Owner")]
public class AdminUsersController(IAdminUserService users) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AdminUserDto>>> List(CancellationToken ct) => await users.ListAsync(ct);

    [HttpPost]
    public async Task<ActionResult<AdminUserDto>> Create(AdminUserCreateDto dto, CancellationToken ct) =>
        await users.CreateAsync(dto, ct);

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AdminUserDto>> Update(int id, AdminUserUpdateDto dto, CancellationToken ct) =>
        await users.UpdateAsync(id, dto, User.UserId(), ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await users.DeleteAsync(id, User.UserId(), ct);
        return NoContent();
    }
}

/// <summary>Tài khoản của khách mua hàng.</summary>
[ApiController]
[Route("api/account")]
public class AccountController(IAccountService account) : ControllerBase
{
    [HttpPost("register")]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<CustomerAuthResult>> Register(CustomerRegisterDto dto, CancellationToken ct) =>
        await account.RegisterAsync(dto, ct);

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<CustomerAuthResult>> Login(CustomerLoginDto dto, CancellationToken ct) =>
        await account.LoginAsync(dto, ct);

    [HttpGet("me")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<CustomerDto>> Me(CancellationToken ct) =>
        await account.MeAsync(User.UserId(), ct);

    [HttpPut("me")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<CustomerDto>> UpdateProfile(CustomerProfileDto dto, CancellationToken ct) =>
        await account.UpdateProfileAsync(User.UserId(), dto, ct);

    [HttpPut("password")]
    [Authorize(Roles = "Customer")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto, CancellationToken ct)
    {
        await account.ChangePasswordAsync(User.UserId(), dto, ct);
        return NoContent();
    }
}
