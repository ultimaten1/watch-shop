using Microsoft.Extensions.DependencyInjection;
using WatchShop.Application.Services;

namespace WatchShop.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IStatsService, StatsService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        return services;
    }
}
