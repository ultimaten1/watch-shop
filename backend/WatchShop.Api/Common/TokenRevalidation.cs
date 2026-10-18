using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WatchShop.Application.Abstractions;
using WatchShop.Domain.Entities;

namespace WatchShop.Api.Common;

/// <summary>
/// Sau khi chữ ký token hợp lệ, đối chiếu lại với database ở mỗi request:
/// tài khoản bị xoá / khoá thì token mất hiệu lực ngay, đổi quyền thì có hiệu lực ngay
/// (token cũ cấp trước khi đổi quyền không còn giữ quyền cũ).
/// </summary>
public static class TokenRevalidation
{
    public static async Task OnTokenValidated(TokenValidatedContext context)
    {
        var principal = context.Principal;
        var identity = principal?.Identity as ClaimsIdentity;
        if (principal is null || identity is null || !principal.UserId().IsPositive())
        {
            context.Fail("Token không hợp lệ");
            return;
        }

        var uow = context.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
        var ct = context.HttpContext.RequestAborted;
        var id = principal.UserId();

        if (principal.IsInRole("Admin"))
        {
            var user = await uow.AdminUsers.GetByIdAsync(id, ct);
            if (user is not { IsActive: true })
            {
                context.Fail("Tài khoản không còn hiệu lực");
                return;
            }
            // Quyền luôn lấy từ database, không tin quyền ghi trong token
            foreach (var c in identity.FindAll(identity.RoleClaimType).ToList()) identity.RemoveClaim(c);
            identity.AddClaim(new Claim(identity.RoleClaimType, "Admin"));
            if (user.Role == AdminRoles.Owner) identity.AddClaim(new Claim(identity.RoleClaimType, AdminRoles.Owner));
        }
        else if (principal.IsInRole("Customer"))
        {
            if (!await uow.Customers.ExistsAsync(id, ct))
                context.Fail("Tài khoản không còn tồn tại");
        }
        else
        {
            context.Fail("Token không có quyền hợp lệ");
        }
    }

    private static bool IsPositive(this int n) => n > 0;
}
