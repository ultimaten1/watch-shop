using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WatchShop.Api.Controllers;

[ApiController]
[Route("api/uploads")]
[Authorize]
public class UploadsController(IWebHostEnvironment env) : ControllerBase
{
    private const long MaxBytes = 5 * 1024 * 1024;
    private static readonly HashSet<string> Allowed = new() { ".jpg", ".jpeg", ".png", ".webp" };

    [HttpPost]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<object>> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "Chưa chọn file" });
        if (file.Length > MaxBytes)
            return BadRequest(new { error = "Ảnh tối đa 5MB" });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Allowed.Contains(ext))
            return BadRequest(new { error = "Chỉ nhận ảnh .jpg, .png, .webp" });

        // Kiểm tra nội dung thật của file, không chỉ tin vào đuôi file
        var header = new byte[12];
        await using (var s = file.OpenReadStream())
            _ = await s.ReadAsync(header.AsMemory(0, 12));
        if (!LooksLikeImage(header, ext))
            return BadRequest(new { error = "File không phải ảnh hợp lệ" });

        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var dir = Path.Combine(webRoot, "uploads");
        Directory.CreateDirectory(dir);

        var name = $"{Guid.NewGuid():N}{ext}";
        await using (var fs = System.IO.File.Create(Path.Combine(dir, name)))
            await file.CopyToAsync(fs);

        return new { url = $"/uploads/{name}" };
    }

    private static bool LooksLikeImage(byte[] h, string ext) => ext switch
    {
        ".jpg" or ".jpeg" => h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF,
        ".png" => h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47,
        ".webp" => h[0] == 'R' && h[1] == 'I' && h[2] == 'F' && h[3] == 'F'
                   && h[8] == 'W' && h[9] == 'E' && h[10] == 'B' && h[11] == 'P',
        _ => false
    };
}
