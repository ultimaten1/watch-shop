namespace WatchShop.Application.Abstractions;

/// <summary>
/// Gom các repository dùng chung một phiên làm việc với database.
/// Mọi thay đổi chỉ được ghi xuống DB một lần (trong một transaction) khi gọi <see cref="SaveChangesAsync"/>.
/// </summary>
public interface IUnitOfWork
{
    IProductRepository Products { get; }
    IBrandRepository Brands { get; }
    IOrderRepository Orders { get; }
    IContactMessageRepository ContactMessages { get; }
    ICustomerRepository Customers { get; }
    IAdminUserRepository AdminUsers { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
