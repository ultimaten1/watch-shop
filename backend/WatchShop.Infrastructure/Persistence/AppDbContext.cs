using Microsoft.EntityFrameworkCore;
using WatchShop.Domain.Entities;

namespace WatchShop.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Brand>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100);
            e.Property(x => x.Slug).HasMaxLength(120);
            e.HasIndex(x => x.Slug).IsUnique();
        });

        b.Entity<Product>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Slug).HasMaxLength(220);
            e.Property(x => x.Gender).HasMaxLength(20);
            e.Property(x => x.MovementType).HasMaxLength(30);
            e.Property(x => x.Material).HasMaxLength(100);
            e.Property(x => x.WaterResistance).HasMaxLength(50);
            e.Property(x => x.ImageUrl).HasMaxLength(500);
            e.Property(x => x.Price).HasPrecision(18, 0);
            e.Property(x => x.OldPrice).HasPrecision(18, 0);
            e.HasIndex(x => x.Slug).IsUnique();
            e.HasOne(x => x.Brand).WithMany(x => x.Products)
                .HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Images).WithOne()
                .HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<ProductImage>(e => e.Property(x => x.Url).HasMaxLength(500));

        b.Entity<Order>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(30);
            e.Property(x => x.CustomerName).HasMaxLength(100);
            e.Property(x => x.Phone).HasMaxLength(20);
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.Address).HasMaxLength(300);
            e.Property(x => x.Note).HasMaxLength(1000);
            e.Property(x => x.Total).HasPrecision(18, 0);
            e.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.Code).IsUnique();
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Items).WithOne()
                .HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<OrderItem>(e =>
        {
            e.Property(x => x.ProductName).HasMaxLength(200);
            e.Property(x => x.UnitPrice).HasPrecision(18, 0);
        });

        b.Entity<ContactMessage>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100);
            e.Property(x => x.Phone).HasMaxLength(20);
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.Message).HasMaxLength(2000);
        });

        b.Entity<Customer>(e =>
        {
            e.Property(x => x.FullName).HasMaxLength(100);
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.PasswordHash).HasMaxLength(100);
            e.Property(x => x.Phone).HasMaxLength(20);
            e.Property(x => x.Address).HasMaxLength(300);
            e.HasIndex(x => x.Email).IsUnique();
        });

        b.Entity<AdminUser>(e =>
        {
            e.Property(x => x.Username).HasMaxLength(50);
            e.Property(x => x.FullName).HasMaxLength(100);
            e.Property(x => x.Role).HasMaxLength(20);
            e.Property(x => x.PasswordHash).HasMaxLength(100);
            e.HasIndex(x => x.Username).IsUnique();
        });
    }
}
