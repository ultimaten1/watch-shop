using WatchShop.Application.Abstractions;
using WatchShop.Application.Common;
using WatchShop.Application.Dtos;
using WatchShop.Domain.Entities;

namespace WatchShop.Application.Services;

public interface IProductService
{
    Task<PagedResult<ProductDto>> ListAsync(ProductQuery query, bool isAdmin, CancellationToken ct = default);
    Task<ProductDto> GetBySlugAsync(string slug, bool isAdmin, CancellationToken ct = default);
    Task<ProductDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(ProductUpsertDto dto, CancellationToken ct = default);
    Task<ProductDto> UpdateAsync(int id, ProductUpsertDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public class ProductService(IUnitOfWork uow) : IProductService
{
    public async Task<PagedResult<ProductDto>> ListAsync(ProductQuery query, bool isAdmin, CancellationToken ct = default)
    {
        query.Page = Math.Max(query.Page, 1);
        query.PageSize = Math.Clamp(query.PageSize, 1, 60);

        var (items, total) = await uow.Products.SearchAsync(query, includeInactive: isAdmin && query.IncludeInactive, ct);
        return new PagedResult<ProductDto>(items.Select(ProductMapper.ToDto).ToList(), total, query.Page, query.PageSize);
    }

    public async Task<ProductDto> GetBySlugAsync(string slug, bool isAdmin, CancellationToken ct = default)
    {
        var p = await uow.Products.GetBySlugAsync(slug, includeInactive: isAdmin, ct)
                ?? throw new NotFoundException();
        return ProductMapper.ToDto(p);
    }

    public async Task<ProductDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var p = await uow.Products.GetWithDetailsAsync(id, track: false, ct) ?? throw new NotFoundException();
        return ProductMapper.ToDto(p);
    }

    public async Task<ProductDto> CreateAsync(ProductUpsertDto dto, CancellationToken ct = default)
    {
        await EnsureBrandAsync(dto.BrandId, ct);

        var p = new Product { CreatedAt = DateTime.UtcNow };
        Apply(p, dto);
        p.Slug = await UniqueSlugAsync(dto.Name, null, ct);

        await uow.Products.AddAsync(p, ct);
        await uow.SaveChangesAsync(ct);

        return await GetByIdAsync(p.Id, ct);
    }

    public async Task<ProductDto> UpdateAsync(int id, ProductUpsertDto dto, CancellationToken ct = default)
    {
        var p = await uow.Products.GetWithDetailsAsync(id, track: true, ct) ?? throw new NotFoundException();
        await EnsureBrandAsync(dto.BrandId, ct);

        Apply(p, dto);
        await uow.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var p = await uow.Products.GetByIdAsync(id, ct) ?? throw new NotFoundException();
        uow.Products.Remove(p);
        await uow.SaveChangesAsync(ct);
    }

    private async Task EnsureBrandAsync(int brandId, CancellationToken ct)
    {
        if (!await uow.Brands.ExistsAsync(brandId, ct))
            throw new BadRequestException("Hãng không tồn tại");
    }

    private static void Apply(Product p, ProductUpsertDto dto)
    {
        p.Name = dto.Name.Trim();
        p.BrandId = dto.BrandId;
        p.Price = dto.Price;
        p.OldPrice = dto.OldPrice;
        p.Description = dto.Description;
        p.Gender = dto.Gender;
        p.MovementType = dto.MovementType;
        p.CaseSizeMm = dto.CaseSizeMm;
        p.Material = dto.Material;
        p.WaterResistance = dto.WaterResistance;
        p.Stock = dto.Stock;
        p.IsFeatured = dto.IsFeatured;
        p.IsActive = dto.IsActive;
        p.ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl;

        p.Images.Clear();
        var order = 0;
        foreach (var url in dto.Images.Where(u => !string.IsNullOrWhiteSpace(u)))
            p.Images.Add(new ProductImage { Url = url, SortOrder = order++ });
    }

    private async Task<string> UniqueSlugAsync(string name, int? ignoreId, CancellationToken ct)
    {
        var baseSlug = Slugger.Make(name);
        var slug = baseSlug;
        var n = 2;
        while (await uow.Products.SlugExistsAsync(slug, ignoreId, ct))
            slug = $"{baseSlug}-{n++}";
        return slug;
    }
}

public interface IBrandService
{
    Task<List<BrandDto>> ListAsync(CancellationToken ct = default);
    Task<BrandDto> CreateAsync(BrandUpsertDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public class BrandService(IUnitOfWork uow) : IBrandService
{
    public async Task<List<BrandDto>> ListAsync(CancellationToken ct = default) =>
        (await uow.Brands.ListWithActiveCountAsync(ct))
        .Select(x => new BrandDto(x.Brand.Id, x.Brand.Name, x.Brand.Slug, x.ActiveProducts)).ToList();

    public async Task<BrandDto> CreateAsync(BrandUpsertDto dto, CancellationToken ct = default)
    {
        var name = dto.Name.Trim();
        var baseSlug = Slugger.Make(name);
        var slug = baseSlug;
        var n = 2;
        while (await uow.Brands.SlugExistsAsync(slug, ct))
            slug = $"{baseSlug}-{n++}";

        var brand = new Brand { Name = name, Slug = slug };
        await uow.Brands.AddAsync(brand, ct);
        await uow.SaveChangesAsync(ct);
        return new BrandDto(brand.Id, brand.Name, brand.Slug, 0);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var brand = await uow.Brands.GetByIdAsync(id, ct) ?? throw new NotFoundException();
        if (await uow.Products.AnyInBrandAsync(id, ct))
            throw new BadRequestException("Hãng còn sản phẩm, hãy xoá hoặc chuyển sản phẩm sang hãng khác trước");
        uow.Brands.Remove(brand);
        await uow.SaveChangesAsync(ct);
    }
}
