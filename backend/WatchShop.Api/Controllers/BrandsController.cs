using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WatchShop.Api.Data;
using WatchShop.Api.Dtos;
using WatchShop.Api.Models;
using WatchShop.Api.Services;

namespace WatchShop.Api.Controllers;

[ApiController]
[Route("api/brands")]
public class BrandsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<BrandDto>>> List()
    {
        return await db.Brands.AsNoTracking()
            .OrderBy(b => b.Name)
            .Select(b => new BrandDto(b.Id, b.Name, b.Slug, b.Products.Count(p => p.IsActive)))
            .ToListAsync();
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<BrandDto>> Create(BrandUpsertDto dto)
    {
        var name = dto.Name.Trim();
        var baseSlug = Slugger.Make(name);
        var slug = baseSlug;
        var n = 2;
        while (await db.Brands.AnyAsync(b => b.Slug == slug))
            slug = $"{baseSlug}-{n++}";

        var brand = new Brand { Name = name, Slug = slug };
        db.Brands.Add(brand);
        await db.SaveChangesAsync();
        return new BrandDto(brand.Id, brand.Name, brand.Slug, 0);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var brand = await db.Brands.FindAsync(id);
        if (brand is null) return NotFound();
        if (await db.Products.AnyAsync(p => p.BrandId == id))
            return BadRequest(new { error = "Hãng còn sản phẩm, hãy xoá hoặc chuyển sản phẩm sang hãng khác trước" });
        db.Brands.Remove(brand);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
