using Microsoft.EntityFrameworkCore;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Dtos;
using WatchShop.Domain.Entities;
using WatchShop.Domain.Enums;

namespace WatchShop.Infrastructure.Persistence.Repositories;

public class Repository<T>(AppDbContext db) : IRepository<T> where T : class
{
    protected AppDbContext Db { get; } = db;
    protected DbSet<T> Set => Db.Set<T>();

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await Set.FindAsync([id], ct);

    public async Task AddAsync(T entity, CancellationToken ct = default) =>
        await Set.AddAsync(entity, ct);

    public void Remove(T entity) => Set.Remove(entity);
}

public class ProductRepository(AppDbContext db) : Repository<Product>(db), IProductRepository
{
    public async Task<(List<Product> Items, int Total)> SearchAsync(
        ProductQuery q, bool includeInactive, CancellationToken ct = default)
    {
        var filtered = Set.AsNoTracking().AsQueryable();

        if (!includeInactive)
            filtered = filtered.Where(p => p.IsActive);
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim();
            filtered = filtered.Where(p => p.Name.Contains(s) || p.Brand!.Name.Contains(s));
        }
        if (!string.IsNullOrWhiteSpace(q.Brand))
            filtered = filtered.Where(p => p.Brand!.Slug == q.Brand);
        if (!string.IsNullOrWhiteSpace(q.Gender))
            filtered = filtered.Where(p => p.Gender == q.Gender);
        if (!string.IsNullOrWhiteSpace(q.Movement))
            filtered = filtered.Where(p => p.MovementType == q.Movement);
        if (q.MinPrice.HasValue)
            filtered = filtered.Where(p => p.Price >= q.MinPrice.Value);
        if (q.MaxPrice.HasValue)
            filtered = filtered.Where(p => p.Price <= q.MaxPrice.Value);
        if (q.Featured == true)
            filtered = filtered.Where(p => p.IsFeatured);

        var total = await filtered.CountAsync(ct);

        filtered = q.Sort switch
        {
            "price_asc" => filtered.OrderBy(p => p.Price),
            "price_desc" => filtered.OrderByDescending(p => p.Price),
            "name" => filtered.OrderBy(p => p.Name),
            _ => filtered.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id)
        };

        var items = await filtered
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<Product?> GetBySlugAsync(string slug, bool includeInactive, CancellationToken ct = default) =>
        Set.AsNoTracking().Include(x => x.Brand).Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Slug == slug && (x.IsActive || includeInactive), ct);

    public Task<Product?> GetWithDetailsAsync(int id, bool track, CancellationToken ct = default)
    {
        var q = Set.Include(x => x.Brand).Include(x => x.Images).AsQueryable();
        if (!track) q = q.AsNoTracking();
        return q.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public Task<bool> SlugExistsAsync(string slug, int? ignoreId, CancellationToken ct = default) =>
        Set.AnyAsync(x => x.Slug == slug && x.Id != ignoreId, ct);

    public Task<bool> AnyInBrandAsync(int brandId, CancellationToken ct = default) =>
        Set.AnyAsync(p => p.BrandId == brandId, ct);

    public Task<List<Product>> GetActiveByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default) =>
        Set.Where(p => ids.Contains(p.Id) && p.IsActive).ToListAsync(ct);

    public Task<List<Product>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default) =>
        Set.Where(p => ids.Contains(p.Id)).ToListAsync(ct);

    public Task<int> CountAsync(CancellationToken ct = default) => Set.CountAsync(ct);

    public Task<int> CountLowStockAsync(int threshold, CancellationToken ct = default) =>
        Set.CountAsync(p => p.Stock <= threshold && p.IsActive, ct);
}

public class BrandRepository(AppDbContext db) : Repository<Brand>(db), IBrandRepository
{
    public async Task<List<(Brand Brand, int ActiveProducts)>> ListWithActiveCountAsync(CancellationToken ct = default)
    {
        var rows = await Set.AsNoTracking()
            .OrderBy(b => b.Name)
            .Select(b => new { Brand = b, Active = b.Products.Count(p => p.IsActive) })
            .ToListAsync(ct);
        return rows.Select(r => (r.Brand, r.Active)).ToList();
    }

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default) => Set.AnyAsync(b => b.Id == id, ct);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default) =>
        Set.AnyAsync(b => b.Slug == slug, ct);
}

public class OrderRepository(AppDbContext db) : Repository<Order>(db), IOrderRepository
{
    public Task<Order?> GetWithItemsAsync(int id, CancellationToken ct = default) =>
        Set.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<Order?> FindByCodeAndPhoneAsync(string code, string phone, CancellationToken ct = default) =>
        Set.AsNoTracking().Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Code == code && o.Phone == phone, ct);

    public Task<List<Order>> ListAsync(OrderStatus? status, int take, CancellationToken ct = default)
    {
        var q = Set.AsNoTracking().Include(o => o.Items).AsQueryable();
        if (status.HasValue) q = q.Where(o => o.Status == status.Value);
        return q.OrderByDescending(o => o.CreatedAt).Take(take).ToListAsync(ct);
    }

    public Task<List<Order>> ListByCustomerAsync(int customerId, int take, CancellationToken ct = default) =>
        Set.AsNoTracking().Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt).Take(take).ToListAsync(ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct = default) =>
        Set.AnyAsync(o => o.Code == code, ct);

    public Task<int> CountByStatusAsync(OrderStatus status, CancellationToken ct = default) =>
        Set.CountAsync(o => o.Status == status, ct);
}

public class ContactMessageRepository(AppDbContext db) : Repository<ContactMessage>(db), IContactMessageRepository
{
    public Task<List<ContactMessage>> ListRecentAsync(int take, CancellationToken ct = default) =>
        Set.AsNoTracking().OrderByDescending(m => m.CreatedAt).Take(take).ToListAsync(ct);

    public Task<int> CountUnreadAsync(CancellationToken ct = default) => Set.CountAsync(m => !m.IsRead, ct);
}

public class CustomerRepository(AppDbContext db) : Repository<Customer>(db), ICustomerRepository
{
    public Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(c => c.Email == email, ct);

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        Set.AnyAsync(c => c.Email == email, ct);

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default) => Set.AnyAsync(c => c.Id == id, ct);
}

public class AdminUserRepository(AppDbContext db) : Repository<AdminUser>(db), IAdminUserRepository
{
    public Task<AdminUser?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(u => u.Username == username, ct);

    public Task<List<AdminUser>> ListAsync(CancellationToken ct = default) =>
        Set.AsNoTracking().OrderBy(u => u.Username).ToListAsync(ct);

    public Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default) =>
        Set.AnyAsync(u => u.Username == username, ct);

    public Task<bool> AnyOtherActiveOwnerAsync(int exceptId, CancellationToken ct = default) =>
        Set.AnyAsync(u => u.Id != exceptId && u.Role == AdminRoles.Owner && u.IsActive, ct);

    public Task<bool> AnyAsync(CancellationToken ct = default) => Set.AnyAsync(ct);
}
