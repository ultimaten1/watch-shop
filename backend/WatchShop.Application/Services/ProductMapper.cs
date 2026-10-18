using WatchShop.Application.Dtos;
using WatchShop.Domain.Entities;

namespace WatchShop.Application.Services;

public static class ProductMapper
{
    /// <summary>Product phải được load kèm Brand và Images.</summary>
    public static ProductDto ToDto(Product p) => new(
        p.Id, p.Name, p.Slug,
        p.BrandId, p.Brand?.Name ?? "", p.Brand?.Slug ?? "",
        p.Price, p.OldPrice, p.Description,
        p.Gender, p.MovementType, p.CaseSizeMm,
        p.Material, p.WaterResistance,
        p.Stock, p.IsFeatured, p.IsActive,
        p.ImageUrl,
        p.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).ToList());
}
