namespace WatchShop.Domain.Entities;

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
