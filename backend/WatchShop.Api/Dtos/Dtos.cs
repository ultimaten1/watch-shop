using System.ComponentModel.DataAnnotations;
using WatchShop.Api.Models;

namespace WatchShop.Api.Dtos;

public record PagedResult<T>(List<T> Items, int Total, int Page, int PageSize);

// ---------- Products ----------
public record ProductDto(
    int Id, string Name, string Slug,
    int BrandId, string BrandName, string BrandSlug,
    decimal Price, decimal? OldPrice, string? Description,
    string Gender, string MovementType, int? CaseSizeMm,
    string? Material, string? WaterResistance,
    int Stock, bool IsFeatured, bool IsActive,
    string? ImageUrl, List<string> Images);

public class ProductQuery
{
    public string? Search { get; set; }
    public string? Brand { get; set; }
    public string? Gender { get; set; }
    public string? Movement { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? Featured { get; set; }
    public bool IncludeInactive { get; set; }
    /// <summary>newest | price_asc | price_desc | name</summary>
    public string? Sort { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public class ProductUpsertDto
{
    [Required, StringLength(200)] public string Name { get; set; } = "";
    [Range(1, int.MaxValue)] public int BrandId { get; set; }
    [Range(0, 1_000_000_000)] public decimal Price { get; set; }
    [Range(0, 1_000_000_000)] public decimal? OldPrice { get; set; }
    [StringLength(5000)] public string? Description { get; set; }
    [StringLength(20)] public string Gender { get; set; } = "Unisex";
    [StringLength(30)] public string MovementType { get; set; } = "Quartz";
    [Range(10, 100)] public int? CaseSizeMm { get; set; }
    [StringLength(100)] public string? Material { get; set; }
    [StringLength(50)] public string? WaterResistance { get; set; }
    [Range(0, 100000)] public int Stock { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    [StringLength(500)] public string? ImageUrl { get; set; }
    public List<string> Images { get; set; } = new();
}

// ---------- Brands ----------
public record BrandDto(int Id, string Name, string Slug, int ProductCount);

public class BrandUpsertDto
{
    [Required, StringLength(100)] public string Name { get; set; } = "";
}

// ---------- Orders ----------
public class OrderItemInput
{
    public int ProductId { get; set; }
    [Range(1, 20)] public int Quantity { get; set; }
}

public class CreateOrderDto
{
    [Required, StringLength(100)] public string CustomerName { get; set; } = "";
    [Required, RegularExpression(@"^(0|\+84)\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string Phone { get; set; } = "";
    [EmailAddress, StringLength(150)] public string? Email { get; set; }
    [Required, StringLength(300)] public string Address { get; set; } = "";
    [StringLength(1000)] public string? Note { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer;
    [Required, MinLength(1), MaxLength(50)] public List<OrderItemInput> Items { get; set; } = new();
}

public record OrderItemDto(int ProductId, string ProductName, decimal UnitPrice, int Quantity);

public record PaymentInfoDto(string BankName, string AccountNumber, string AccountName,
    string TransferContent, string QrUrl);

public record OrderDto(
    int Id, string Code, string CustomerName, string Phone, string? Email,
    string Address, string? Note, PaymentMethod PaymentMethod, OrderStatus Status,
    decimal Total, DateTime CreatedAt, List<OrderItemDto> Items, PaymentInfoDto? Payment);

public class UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
}

// ---------- Contact ----------
public class ContactCreateDto
{
    [Required, StringLength(100)] public string Name { get; set; } = "";
    [Required, RegularExpression(@"^(0|\+84)\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string Phone { get; set; } = "";
    [EmailAddress, StringLength(150)] public string? Email { get; set; }
    [Required, StringLength(2000)] public string Message { get; set; } = "";
}

// ---------- Auth ----------
public class LoginDto
{
    [Required] public string Username { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}

public record LoginResult(string Token, DateTime ExpiresAt);
