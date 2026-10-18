using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using WatchShop.Api.Data;
using WatchShop.Api.Dtos;
using WatchShop.Api.Models;

namespace WatchShop.Api.Controllers;

[ApiController]
[Route("api/contact")]
public class ContactController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("forms")]
    public async Task<IActionResult> Send(ContactCreateDto dto)
    {
        db.ContactMessages.Add(new ContactMessage
        {
            Name = dto.Name.Trim(),
            Phone = dto.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
            Message = dto.Message.Trim(),
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return Ok(new { ok = true });
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<ContactMessage>>> List()
        => await db.ContactMessages.AsNoTracking()
            .OrderByDescending(m => m.CreatedAt).Take(200).ToListAsync();

    [HttpPut("{id:int}/read")]
    [Authorize]
    public async Task<IActionResult> MarkRead(int id)
    {
        var m = await db.ContactMessages.FindAsync(id);
        if (m is null) return NotFound();
        m.IsRead = true;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var m = await db.ContactMessages.FindAsync(id);
        if (m is null) return NotFound();
        db.ContactMessages.Remove(m);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
