using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WatchShop.Application.Abstractions;
using WatchShop.Infrastructure.Persistence;
using WatchShop.Infrastructure.Security;

namespace WatchShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(o =>
            o.UseSqlServer(config.GetConnectionString("Default"),
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Một UnitOfWork (và một DbContext) cho mỗi request
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        return services;
    }

    /// <summary>Tạo / cập nhật database theo migration và nạp dữ liệu mẫu lần đầu.</summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (db.Database.GetMigrations().Any())
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync();
        await DbSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<IConfiguration>());
    }
}
