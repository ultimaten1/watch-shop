using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WatchShop.Api.Data;
using WatchShop.Api.Dtos;
using WatchShop.Api.Models;

namespace WatchShop.Api.Controllers;

/// <summary>Quản lý tài khoản nhân viên. Chỉ chủ cửa hàng (Owner) dùng được.</summary>
[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Owner")]
public class AdminUsersController(AppDbContext db) : ControllerBase
{
    private static AdminUserDto ToDto(AdminUser u) => new(u.Id, u.Username, u.FullName, u.Role, u.IsActive);

    private static bool ValidRole(string r) => r is AdminRoles.Owner or AdminRoles.Staff;

    private int CurrentId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    [HttpGet]
    public async Task<List<AdminUserDto>> List() =>
        (await db.AdminUsers.AsNoTracking().OrderBy(u => u.Username).ToListAsync()).Select(ToDto).ToList();

    [HttpPost]
    public async Task<ActionResult<AdminUserDto>> Create(AdminUserCreateDto dto)
    {
        var username = dto.Username.Trim();
        if (!ValidRole(dto.Role)) return BadRequest(new { error = "Quyền không hợp lệ" });
        if (await db.AdminUsers.AnyAsync(u => u.Username == username))
            return BadRequest(new { error = "Tên đăng nhập đã tồn tại" });

        var user = new AdminUser
        {
            Username = username,
            FullName = dto.FullName.Trim(),
            Role = dto.Role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };
        db.AdminUsers.Add(user);
        await db.SaveChangesAsync();
        return ToDto(user);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AdminUserDto>> Update(int id, AdminUserUpdateDto dto)
    {
        var user = await db.AdminUsers.FindAsync(id);
        if (user is null) return NotFound();
        if (!ValidRole(dto.Role)) return BadRequest(new { error = "Quyền không hợp lệ" });

        // Không cho tự hạ quyền / khoá chính mình, và luôn còn ít nhất một chủ cửa hàng đang hoạt động
        var losesOwner = user.Role == AdminRoles.Owner && user.IsActive
                         && (dto.Role != AdminRoles.Owner || !dto.IsActive);
        if (losesOwner)
        {
            if (user.Id == CurrentId)
                return BadRequest(new { error = "Bạn không thể tự hạ quyền hoặc khoá chính mình" });
            var otherOwners = await db.AdminUsers.CountAsync(u =>
                u.Id != id && u.Role == AdminRoles.Owner && u.IsActive);
            if (otherOwners == 0)
                return BadRequest(new { error = "Phải còn ít nhất một chủ cửa hàng đang hoạt động" });
        }

        user.FullName = dto.FullName.Trim();
        user.Role = dto.Role;
        user.IsActive = dto.IsActive;
        if (!string.IsNullOrEmpty(dto.NewPassword))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await db.SaveChangesAsync();
        return ToDto(user);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await db.AdminUsers.FindAsync(id);
        if (user is null) return NotFound();
        if (user.Id == CurrentId) return BadRequest(new { error = "Bạn không thể xoá chính mình" });
        if (user.Role == AdminRoles.Owner && user.IsActive
            && !await db.AdminUsers.AnyAsync(u => u.Id != id && u.Role == AdminRoles.Owner && u.IsActive))
            return BadRequest(new { error = "Phải còn ít nhất một chủ cửa hàng đang hoạt động" });

        db.AdminUsers.Remove(user);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
