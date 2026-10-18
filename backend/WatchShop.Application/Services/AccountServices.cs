using WatchShop.Application.Abstractions;
using WatchShop.Application.Common;
using WatchShop.Application.Dtos;
using WatchShop.Domain.Entities;

namespace WatchShop.Application.Services;

// ======================= Đăng nhập quản trị =======================
public interface IAuthService
{
    Task<LoginResult> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<AdminMeDto> MeAsync(int userId, CancellationToken ct = default);
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto, CancellationToken ct = default);
}

public class AuthService(IUnitOfWork uow, IPasswordHasher hasher, ITokenService tokens) : IAuthService
{
    public async Task<LoginResult> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await uow.AdminUsers.GetByUsernameAsync(dto.Username, ct);
        if (user is null || !user.IsActive || !hasher.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedException("Sai tên đăng nhập hoặc mật khẩu");

        var t = tokens.ForAdmin(user);
        return new LoginResult(t.Token, t.ExpiresAt);
    }

    public async Task<AdminMeDto> MeAsync(int userId, CancellationToken ct = default)
    {
        var user = await ActiveAsync(userId, ct);
        return new AdminMeDto(user.Username, user.FullName, user.Role);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto, CancellationToken ct = default)
    {
        var user = await ActiveAsync(userId, ct);
        if (!hasher.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new BadRequestException("Mật khẩu hiện tại không đúng");

        user.PasswordHash = hasher.Hash(dto.NewPassword);
        await uow.SaveChangesAsync(ct);
    }

    private async Task<AdminUser> ActiveAsync(int userId, CancellationToken ct)
    {
        var user = await uow.AdminUsers.GetByIdAsync(userId, ct);
        return user is { IsActive: true } ? user : throw new UnauthorizedException("Phiên đăng nhập không hợp lệ");
    }
}

// ======================= Tài khoản khách =======================
public interface IAccountService
{
    Task<CustomerAuthResult> RegisterAsync(CustomerRegisterDto dto, CancellationToken ct = default);
    Task<CustomerAuthResult> LoginAsync(CustomerLoginDto dto, CancellationToken ct = default);
    Task<CustomerDto> MeAsync(int customerId, CancellationToken ct = default);
    Task<CustomerDto> UpdateProfileAsync(int customerId, CustomerProfileDto dto, CancellationToken ct = default);
    Task ChangePasswordAsync(int customerId, ChangePasswordDto dto, CancellationToken ct = default);
}

public class AccountService(IUnitOfWork uow, IPasswordHasher hasher, ITokenService tokens) : IAccountService
{
    private static CustomerDto ToDto(Customer c) => new(c.Id, c.FullName, c.Email, c.Phone, c.Address);
    private static string NormalizeEmail(string e) => e.Trim().ToLowerInvariant();

    public async Task<CustomerAuthResult> RegisterAsync(CustomerRegisterDto dto, CancellationToken ct = default)
    {
        var email = NormalizeEmail(dto.Email);
        if (await uow.Customers.EmailExistsAsync(email, ct))
            throw new BadRequestException("Email này đã được đăng ký. Hãy đăng nhập.");

        var customer = new Customer
        {
            FullName = dto.FullName.Trim(),
            Email = email,
            Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim(),
            PasswordHash = hasher.Hash(dto.Password)
        };
        await uow.Customers.AddAsync(customer, ct);
        await uow.SaveChangesAsync(ct);

        var t = tokens.ForCustomer(customer);
        return new CustomerAuthResult(t.Token, t.ExpiresAt, ToDto(customer));
    }

    public async Task<CustomerAuthResult> LoginAsync(CustomerLoginDto dto, CancellationToken ct = default)
    {
        var customer = await uow.Customers.GetByEmailAsync(NormalizeEmail(dto.Email), ct);
        if (customer is null || !hasher.Verify(dto.Password, customer.PasswordHash))
            throw new UnauthorizedException("Sai email hoặc mật khẩu");

        var t = tokens.ForCustomer(customer);
        return new CustomerAuthResult(t.Token, t.ExpiresAt, ToDto(customer));
    }

    public async Task<CustomerDto> MeAsync(int customerId, CancellationToken ct = default) =>
        ToDto(await GetAsync(customerId, ct));

    public async Task<CustomerDto> UpdateProfileAsync(int customerId, CustomerProfileDto dto, CancellationToken ct = default)
    {
        var c = await GetAsync(customerId, ct);
        c.FullName = dto.FullName.Trim();
        c.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        c.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();
        await uow.SaveChangesAsync(ct);
        return ToDto(c);
    }

    public async Task ChangePasswordAsync(int customerId, ChangePasswordDto dto, CancellationToken ct = default)
    {
        var c = await GetAsync(customerId, ct);
        if (!hasher.Verify(dto.CurrentPassword, c.PasswordHash))
            throw new BadRequestException("Mật khẩu hiện tại không đúng");
        c.PasswordHash = hasher.Hash(dto.NewPassword);
        await uow.SaveChangesAsync(ct);
    }

    private async Task<Customer> GetAsync(int id, CancellationToken ct) =>
        await uow.Customers.GetByIdAsync(id, ct) ?? throw new UnauthorizedException("Phiên đăng nhập không hợp lệ");
}

// ======================= Quản lý tài khoản nhân viên (Owner) =======================
public interface IAdminUserService
{
    Task<List<AdminUserDto>> ListAsync(CancellationToken ct = default);
    Task<AdminUserDto> CreateAsync(AdminUserCreateDto dto, CancellationToken ct = default);
    Task<AdminUserDto> UpdateAsync(int id, AdminUserUpdateDto dto, int currentUserId, CancellationToken ct = default);
    Task DeleteAsync(int id, int currentUserId, CancellationToken ct = default);
}

public class AdminUserService(IUnitOfWork uow, IPasswordHasher hasher) : IAdminUserService
{
    private static AdminUserDto ToDto(AdminUser u) => new(u.Id, u.Username, u.FullName, u.Role, u.IsActive);
    private static bool ValidRole(string r) => r is AdminRoles.Owner or AdminRoles.Staff;

    public async Task<List<AdminUserDto>> ListAsync(CancellationToken ct = default) =>
        (await uow.AdminUsers.ListAsync(ct)).Select(ToDto).ToList();

    public async Task<AdminUserDto> CreateAsync(AdminUserCreateDto dto, CancellationToken ct = default)
    {
        var username = dto.Username.Trim();
        if (!ValidRole(dto.Role)) throw new BadRequestException("Quyền không hợp lệ");
        if (await uow.AdminUsers.UsernameExistsAsync(username, ct))
            throw new BadRequestException("Tên đăng nhập đã tồn tại");

        var user = new AdminUser
        {
            Username = username,
            FullName = dto.FullName.Trim(),
            Role = dto.Role,
            PasswordHash = hasher.Hash(dto.Password)
        };
        await uow.AdminUsers.AddAsync(user, ct);
        await uow.SaveChangesAsync(ct);
        return ToDto(user);
    }

    public async Task<AdminUserDto> UpdateAsync(int id, AdminUserUpdateDto dto, int currentUserId, CancellationToken ct = default)
    {
        var user = await uow.AdminUsers.GetByIdAsync(id, ct) ?? throw new NotFoundException();
        if (!ValidRole(dto.Role)) throw new BadRequestException("Quyền không hợp lệ");

        // Không cho tự hạ quyền / khoá chính mình, và luôn còn ít nhất một chủ cửa hàng đang hoạt động
        var losesOwner = user.Role == AdminRoles.Owner && user.IsActive
                         && (dto.Role != AdminRoles.Owner || !dto.IsActive);
        if (losesOwner)
        {
            if (user.Id == currentUserId)
                throw new BadRequestException("Bạn không thể tự hạ quyền hoặc khoá chính mình");
            if (!await uow.AdminUsers.AnyOtherActiveOwnerAsync(id, ct))
                throw new BadRequestException("Phải còn ít nhất một chủ cửa hàng đang hoạt động");
        }

        user.FullName = dto.FullName.Trim();
        user.Role = dto.Role;
        user.IsActive = dto.IsActive;
        if (!string.IsNullOrEmpty(dto.NewPassword))
            user.PasswordHash = hasher.Hash(dto.NewPassword);
        await uow.SaveChangesAsync(ct);
        return ToDto(user);
    }

    public async Task DeleteAsync(int id, int currentUserId, CancellationToken ct = default)
    {
        var user = await uow.AdminUsers.GetByIdAsync(id, ct) ?? throw new NotFoundException();
        if (user.Id == currentUserId) throw new BadRequestException("Bạn không thể xoá chính mình");
        if (user.Role == AdminRoles.Owner && user.IsActive && !await uow.AdminUsers.AnyOtherActiveOwnerAsync(id, ct))
            throw new BadRequestException("Phải còn ít nhất một chủ cửa hàng đang hoạt động");

        uow.AdminUsers.Remove(user);
        await uow.SaveChangesAsync(ct);
    }
}
