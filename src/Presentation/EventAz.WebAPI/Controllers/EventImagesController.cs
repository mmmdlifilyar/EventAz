using System.Security.Claims;
using EventAz.Domain.Entities;
using EventAz.Domain.Enums;
using EventAz.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventAz.WebAPI.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}/image")]
public sealed class EventImagesController : ControllerBase
{
    private const long MaxImageSize = 5 * 1024 * 1024;
    private const long MaxRequestSize = MaxImageSize + 64 * 1024;
    private static readonly string[] Extensions = [".jpg", ".png", ".webp"];
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public EventImagesController(ApplicationDbContext db, IWebHostEnvironment environment)
        => (_db, _environment) = (db, environment);

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid eventId, CancellationToken ct)
    {
        var eventEntity = await _db.Events.AsNoTracking().FirstOrDefaultAsync(item => item.Id == eventId, ct);
        if (eventEntity is null || !CanView(eventEntity)) return NotFound();

        var directory = GetImageDirectory();
        foreach (var extension in Extensions)
        {
            var path = Path.Combine(directory, $"{eventId:N}{extension}");
            if (System.IO.File.Exists(path))
                return PhysicalFile(path, GetContentType(extension), enableRangeProcessing: true);
        }

        return NoContent();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Organizer")]
    [RequestSizeLimit(MaxRequestSize)]
    public async Task<IActionResult> Upload(Guid eventId, IFormFile? file, CancellationToken ct)
    {
        var eventEntity = await _db.Events.FirstOrDefaultAsync(item => item.Id == eventId, ct);
        if (eventEntity is null) return NotFound();
        if (!User.IsInRole("Admin") && User.FindFirstValue(ClaimTypes.NameIdentifier) != eventEntity.OrganizerId)
            return Forbid();
        if (file is null || file.Length == 0) return BadRequest(new { message = "Şəkil faylı seçilməyib." });
        if (file.Length > MaxImageSize) return BadRequest(new { message = "Şəkil 5 MB-dan kiçik olmalıdır." });

        await using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, ct);
        var bytes = buffer.ToArray();
        var extension = GetValidatedExtension(file.ContentType, bytes);
        if (extension is null)
            return BadRequest(new { message = "Yalnız JPG, PNG və WebP şəkilləri yükləmək olar." });

        var directory = GetImageDirectory();
        Directory.CreateDirectory(directory);
        var targetPath = Path.Combine(directory, $"{eventId:N}{extension}");
        var temporaryPath = Path.Combine(directory, $"{Guid.NewGuid():N}.upload");
        try
        {
            await System.IO.File.WriteAllBytesAsync(temporaryPath, bytes, ct);
            System.IO.File.Move(temporaryPath, targetPath, overwrite: true);
            foreach (var otherExtension in Extensions.Where(value => value != extension))
            {
                var oldPath = Path.Combine(directory, $"{eventId:N}{otherExtension}");
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
            }
        }
        finally
        {
            if (System.IO.File.Exists(temporaryPath)) System.IO.File.Delete(temporaryPath);
        }

        return NoContent();
    }

    private bool CanView(Event eventEntity)
        => eventEntity.Status == EventStatus.Published || User.IsInRole("Admin") ||
           User.FindFirstValue(ClaimTypes.NameIdentifier) == eventEntity.OrganizerId;

    private string GetImageDirectory()
        => Path.Combine(_environment.ContentRootPath, "wwwroot", "event-images");

    private static string GetContentType(string extension) => extension switch
    {
        ".jpg" => "image/jpeg",
        ".png" => "image/png",
        _ => "image/webp"
    };

    private static string? GetValidatedExtension(string contentType, byte[] bytes)
    {
        if (contentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase) && bytes.Length >= 3 &&
            bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF) return ".jpg";
        if (contentType.Equals("image/png", StringComparison.OrdinalIgnoreCase) && bytes.Length >= 8 &&
            bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })) return ".png";
        if (contentType.Equals("image/webp", StringComparison.OrdinalIgnoreCase) && bytes.Length >= 12 &&
            bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8) && bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8)) return ".webp";
        return null;
    }
}
