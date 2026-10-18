using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchShop.Api.Common;
using WatchShop.Application.Dtos;
using WatchShop.Application.Services;

namespace WatchShop.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(IProductService products) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> List([FromQuery] ProductQuery q, CancellationToken ct) =>
        await products.ListAsync(q, User.IsAdmin(), ct);

    [HttpGet("{slug}")]
    public async Task<ActionResult<ProductDto>> GetBySlug(string slug, CancellationToken ct) =>
        await products.GetBySlugAsync(slug, User.IsAdmin(), ct);

    [HttpGet("by-id/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken ct) =>
        await products.GetByIdAsync(id, ct);

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> Create(ProductUpsertDto dto, CancellationToken ct)
    {
        var created = await products.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetBySlug), new { slug = created.Slug }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> Update(int id, ProductUpsertDto dto, CancellationToken ct) =>
        await products.UpdateAsync(id, dto, ct);

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await products.DeleteAsync(id, ct);
        return NoContent();
    }
}

[ApiController]
[Route("api/brands")]
public class BrandsController(IBrandService brands) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<BrandDto>>> List(CancellationToken ct) => await brands.ListAsync(ct);

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BrandDto>> Create(BrandUpsertDto dto, CancellationToken ct) =>
        await brands.CreateAsync(dto, ct);

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await brands.DeleteAsync(id, ct);
        return NoContent();
    }
}
