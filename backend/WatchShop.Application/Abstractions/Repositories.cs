using WatchShop.Application.Dtos;
using WatchShop.Domain.Entities;
using WatchShop.Domain.Enums;

namespace WatchShop.Application.Abstractions;

/// <summary>Thao tác chung cho mọi thực thể. Chỉ thay đổi trong bộ nhớ, lưu xuống DB qua <see cref="IUnitOfWork"/>.</summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Remove(T entity);
}

public interface IProductRepository : IRepository<Product>
{
    Task<(List<Product> Items, int Total)> SearchAsync(ProductQuery query, bool includeInactive, CancellationToken ct = default);
    Task<Product?> GetBySlugAsync(string slug, bool includeInactive, CancellationToken ct = default);
    /// <summary>Kèm Brand và Images, theo dõi thay đổi để cập nhật.</summary>
    Task<Product?> GetWithDetailsAsync(int id, bool track, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, int? ignoreId, CancellationToken ct = default);
    Task<bool> AnyInBrandAsync(int brandId, CancellationToken ct = default);
    /// <summary>Sản phẩm đang bán theo danh sách id, theo dõi thay đổi (để trừ kho).</summary>
    Task<List<Product>> GetActiveByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
    Task<List<Product>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
    Task<int> CountLowStockAsync(int threshold, CancellationToken ct = default);
}

public interface IBrandRepository : IRepository<Brand>
{
    Task<List<(Brand Brand, int ActiveProducts)>> ListWithActiveCountAsync(CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
}

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetWithItemsAsync(int id, CancellationToken ct = default);
    Task<Order?> FindByCodeAndPhoneAsync(string code, string phone, CancellationToken ct = default);
    Task<List<Order>> ListAsync(OrderStatus? status, int take, CancellationToken ct = default);
    Task<List<Order>> ListByCustomerAsync(int customerId, int take, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task<int> CountByStatusAsync(OrderStatus status, CancellationToken ct = default);
}

public interface IContactMessageRepository : IRepository<ContactMessage>
{
    Task<List<ContactMessage>> ListRecentAsync(int take, CancellationToken ct = default);
    Task<int> CountUnreadAsync(CancellationToken ct = default);
}

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
}

public interface IAdminUserRepository : IRepository<AdminUser>
{
    Task<AdminUser?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<List<AdminUser>> ListAsync(CancellationToken ct = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);
    Task<bool> AnyOtherActiveOwnerAsync(int exceptId, CancellationToken ct = default);
    Task<bool> AnyAsync(CancellationToken ct = default);
}
