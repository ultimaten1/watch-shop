using WatchShop.Application.Abstractions;
using WatchShop.Infrastructure.Persistence.Repositories;

namespace WatchShop.Infrastructure.Persistence;

/// <summary>
/// Các repository trong cùng một UnitOfWork dùng chung một AppDbContext,
/// nên <see cref="SaveChangesAsync"/> ghi mọi thay đổi trong một transaction.
/// </summary>
public class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    private IProductRepository? _products;
    private IBrandRepository? _brands;
    private IOrderRepository? _orders;
    private IContactMessageRepository? _messages;
    private ICustomerRepository? _customers;
    private IAdminUserRepository? _adminUsers;

    public IProductRepository Products => _products ??= new ProductRepository(db);
    public IBrandRepository Brands => _brands ??= new BrandRepository(db);
    public IOrderRepository Orders => _orders ??= new OrderRepository(db);
    public IContactMessageRepository ContactMessages => _messages ??= new ContactMessageRepository(db);
    public ICustomerRepository Customers => _customers ??= new CustomerRepository(db);
    public IAdminUserRepository AdminUsers => _adminUsers ??= new AdminUserRepository(db);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
