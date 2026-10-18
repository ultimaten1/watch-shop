using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WatchShop.Api.Data;
using WatchShop.Api.Dtos;
using WatchShop.Api.Models;
using WatchShop.Api.Services;

namespace WatchShop.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(AppDbContext db) : ControllerBase
{
    private bool IsAdmin => User.Identity?.IsAuthenticated == true;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> List([FromQuery] ProductQuery q)
    {
        var page = Math.Max(q.Page, 1);
        var pageSize = Math.Clamp(q.PageSize, 1, 60);

        var filtered = db.Products.AsNoTracking().AsQueryable();

        if (!(IsAdmin && q.IncludeInactive))
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

        var total = await filtered.CountAsync();

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
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ProductDto>(items.Select(ProductMapper.ToDto).ToList(), total, page, pageSize);
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<ProductDto>> GetBySlug(string slug)
    {
        var p = await db.Products.AsNoTracking()
            .Include(x => x.Brand).Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Slug == slug && (x.IsActive || IsAdmin));
        return p is null ? NotFound() : ProductMapper.ToDto(p);
    }

    [HttpGet("by-id/{id:int}")]
    [Authorize]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var p = await db.Products.AsNoTracking()
            .Include(x => x.Brand).Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id);
        return p is null ? NotFound() : ProductMapper.ToDto(p);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ProductDto>> Create(ProductUpsertDto dto)
    {
        if (!await db.Brands.AnyAsync(b => b.Id == dto.BrandId))
            return BadRequest(new { error = "Hãng không tồn tại" });

        var p = new Product { CreatedAt = DateTime.UtcNow };
        Apply(p, dto);
        p.Slug = await UniqueSlugAsync(dto.Name, null);

        db.Products.Add(p);
        await db.SaveChangesAsync();
        await db.Entry(p).Reference(x => x.Brand).LoadAsync();

        return CreatedAtAction(nameof(GetBySlug), new { slug = p.Slug }, ProductMapper.ToDto(p));
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<ProductDto>> Update(int id, ProductUpsertDto dto)
    {
        var p = await db.Products.Include(x => x.Images).Include(x => x.Brand)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (p is null) return NotFound();
        if (!await db.Brands.AnyAsync(b => b.Id == dto.BrandId))
            return BadRequest(new { error = "Hãng không tồn tại" });

        Apply(p, dto);
        await db.SaveChangesAsync();
        await db.Entry(p).Reference(x => x.Brand).LoadAsync();
        return ProductMapper.ToDto(p);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await db.Products.FindAsync(id);
        if (p is null) return NotFound();
        db.Products.Remove(p);
        await db.SaveChangesAsync();
        return NoContent();
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

    private async Task<string> UniqueSlugAsync(string name, int? ignoreId)
    {
        var baseSlug = Slugger.Make(name);
        var slug = baseSlug;
        var n = 2;
        while (await db.Products.AnyAsync(x => x.Slug == slug && x.Id != ignoreId))
            slug = $"{baseSlug}-{n++}";
        return slug;
    }
}
