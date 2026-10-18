namespace WatchShop.Api.Models;

public enum PaymentMethod { BankTransfer, Cod, ContactLater }

public enum OrderStatus { New, Confirmed, Paid, Shipping, Completed, Cancelled }

public class Brand
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public List<Product> Products { get; set; } = new();
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public int BrandId { get; set; }
    public Brand? Brand { get; set; }
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public string? Description { get; set; }
    /// <summary>Nam | Nữ | Unisex</summary>
    public string Gender { get; set; } = "Unisex";
    /// <summary>Automatic | Quartz | Solar</summary>
    public string MovementType { get; set; } = "Quartz";
    public int? CaseSizeMm { get; set; }
    public string? Material { get; set; }
    public string? WaterResistance { get; set; }
    public int Stock { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ImageUrl { get; set; }
    public List<ProductImage> Images { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Url { get; set; } = "";
    public int SortOrder { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Email { get; set; }
    public string Address { get; set; } = "";
    public string? Note { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

public class ContactMessage
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Email { get; set; }
    public string Message { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
}

public class AdminUser
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
}
