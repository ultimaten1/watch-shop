using Microsoft.EntityFrameworkCore;
using WatchShop.Api.Models;
using WatchShop.Api.Services;

namespace WatchShop.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IConfiguration config)
    {
        // Tài khoản quản trị đầu tiên (lấy từ appsettings "Admin")
        if (!await db.AdminUsers.AnyAsync())
        {
            var username = config["Admin:Username"] ?? "admin";
            var password = config["Admin:Password"] ?? "ChangeMe@123";
            db.AdminUsers.Add(new AdminUser
            {
                Username = username,
                FullName = "Chủ cửa hàng",
                Role = AdminRoles.Owner,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            });
            await db.SaveChangesAsync();
        }

        // Dữ liệu mẫu: xoá hoặc sửa trong trang quản trị sau khi chạy
        if (await db.Brands.AnyAsync()) return;

        var brands = new[] { "Seiko", "Citizen", "Casio", "Orient", "Tissot" }
            .Select(n => new Brand { Name = n, Slug = Slugger.Make(n) })
            .ToList();
        db.Brands.AddRange(brands);
        await db.SaveChangesAsync();

        Brand B(string name) => brands.First(b => b.Name == name);

        var samples = new[]
        {
            new Product { Name = "Đồng hồ cơ Automatic mặt xanh 40mm", BrandId = B("Seiko").Id, Price = 6_900_000, OldPrice = 7_500_000, Gender = "Nam", MovementType = "Automatic", CaseSizeMm = 40, Material = "Thép không gỉ", WaterResistance = "10 ATM", Stock = 5, IsFeatured = true,
                Description = "Mẫu demo. Máy cơ tự động, dây thép, mặt kính chống trầy. Thay mô tả thật trong trang quản trị." },
            new Product { Name = "Đồng hồ năng lượng ánh sáng mặt đen", BrandId = B("Citizen").Id, Price = 5_400_000, Gender = "Nam", MovementType = "Solar", CaseSizeMm = 42, Material = "Thép không gỉ", WaterResistance = "10 ATM", Stock = 8, IsFeatured = true,
                Description = "Mẫu demo. Chạy bằng năng lượng ánh sáng, không cần thay pin." },
            new Product { Name = "Đồng hồ điện tử phong cách cổ điển", BrandId = B("Casio").Id, Price = 1_250_000, Gender = "Unisex", MovementType = "Quartz", CaseSizeMm = 35, Material = "Nhựa resin", WaterResistance = "3 ATM", Stock = 20, IsFeatured = true,
                Description = "Mẫu demo. Nhẹ, bền, phù hợp dùng hằng ngày." },
            new Product { Name = "Đồng hồ cơ lộ máy dây da nâu", BrandId = B("Orient").Id, Price = 8_200_000, Gender = "Nam", MovementType = "Automatic", CaseSizeMm = 41, Material = "Thép không gỉ", WaterResistance = "5 ATM", Stock = 3, IsFeatured = true,
                Description = "Mẫu demo. Mặt lộ cơ cấu máy, dây da thật." },
            new Product { Name = "Đồng hồ nữ mặt nhỏ dây lưới", BrandId = B("Tissot").Id, Price = 9_800_000, Gender = "Nữ", MovementType = "Quartz", CaseSizeMm = 28, Material = "Thép không gỉ", WaterResistance = "3 ATM", Stock = 4,
                Description = "Mẫu demo. Thiết kế thanh lịch, dây lưới mềm." },
            new Product { Name = "Đồng hồ nữ mặt trắng viền vàng", BrandId = B("Citizen").Id, Price = 4_300_000, OldPrice = 4_900_000, Gender = "Nữ", MovementType = "Quartz", CaseSizeMm = 30, Material = "Thép mạ vàng", WaterResistance = "5 ATM", Stock = 6,
                Description = "Mẫu demo." },
        };

        foreach (var p in samples)
        {
            p.Slug = Slugger.Make(p.Name);
            p.CreatedAt = DateTime.UtcNow;
        }
        db.Products.AddRange(samples);
        await db.SaveChangesAsync();
    }
}
