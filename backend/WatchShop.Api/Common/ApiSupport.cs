using System.Security.Claims;
using WatchShop.Application.Common;

namespace WatchShop.Api.Common;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Id người dùng trong token (admin hoặc khách), 0 nếu không có.</summary>
    public static int UserId(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole("Admin");
}

/// <summary>Chuyển lỗi nghiệp vụ từ lớp Application thành mã HTTP và { error } cho frontend.</summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex) when (!context.Response.HasStarted)
        {
            context.Response.StatusCode = ex switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status400BadRequest
            };
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            logger.LogError(ex, "Lỗi không xử lý được: {Method} {Path}", context.Request.Method, context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "Có lỗi xảy ra phía máy chủ, vui lòng thử lại sau." });
        }
    }
}
