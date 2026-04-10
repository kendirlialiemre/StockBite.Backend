using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockBite.Api.Filters;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Menu.Commands;
using StockBite.Application.Menu.Queries;
using StockBite.Domain.Constants;
using StockBite.Domain.Enums;
using System.Net.Http;

namespace StockBite.Api.Controllers.Menu;

public record UpdateMenuSettingsRequest(int? QrMenuTemplate, string? PrimaryColor, string? BgColor, string? TextColor, string? FontFamily);
public record UpdateQrCodeDesignRequest(int? QrMenuTemplate, string? PrimaryColor, string? BgColor, string? TextColor, string? FontFamily);

public record UpdateCategoryRequest(string? Name, int? DisplayOrder, bool? IsActive);

public record IngredientRequestDto(Guid StockItemId, decimal Quantity);

public class UpdateItemRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public decimal? CostPrice { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? IsAvailable { get; set; }
    public List<IngredientRequestDto>? Ingredients { get; set; }
}

[ApiController]
[Route("api/menu")]
[Authorize]
[RequireModule(ModuleType.Menu)]
public class MenuController(IMediator mediator, IApplicationDbContext db, IStorageService storage) : ControllerBase
{
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings(CancellationToken ct) =>
        Ok(await mediator.Send(new GetMenuSettingsQuery(), ct));

    [HttpPatch("settings")]
    [RequirePermission(Permissions.Menu.Edit)]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateMenuSettingsRequest req, CancellationToken ct)
    {
        await mediator.Send(new SaveMenuDesignCommand(req.QrMenuTemplate, req.PrimaryColor, req.BgColor, req.TextColor, req.FontFamily), ct);
        return NoContent();
    }

    [HttpGet("qr-codes/{id:guid}/design")]
    public async Task<IActionResult> GetQrDesign(Guid id, CancellationToken ct) =>
        Ok(await mediator.Send(new GetQrCodeDesignQuery(id), ct));

    [HttpPatch("qr-codes/{id:guid}/design")]
    [RequirePermission(Permissions.Menu.Edit)]
    public async Task<IActionResult> UpdateQrDesign(Guid id, [FromBody] UpdateQrCodeDesignRequest req, CancellationToken ct)
    {
        await mediator.Send(new SaveQrCodeDesignCommand(id, req.QrMenuTemplate, req.PrimaryColor, req.BgColor, req.TextColor, req.FontFamily), ct);
        return NoContent();
    }

    [HttpPost("qr-codes/{id:guid}/design/logo")]
    [RequirePermission(Permissions.Menu.Edit)]
    public async Task<IActionResult> UploadQrLogo(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0) return BadRequest(new { message = "Dosya seçilmedi." });
        var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowed.Contains(file.ContentType)) return BadRequest(new { message = "Sadece JPEG, PNG veya WebP." });
        if (file.Length > 2 * 1024 * 1024) return BadRequest(new { message = "Maks 2MB." });

        var qr = await db.MenuQrCodes.FirstOrDefaultAsync(q => q.Id == id, ct);
        if (qr == null) return NotFound();

        if (!string.IsNullOrEmpty(qr.LogoUrl))
            await storage.DeleteAsync(qr.LogoUrl, ct);

        using var stream = file.OpenReadStream();
        var logoUrl = await storage.UploadAsync(stream, file.FileName, file.ContentType, "logos", ct);

        qr.LogoUrl = logoUrl;
        await db.SaveChangesAsync(ct);
        return Ok(new { logoUrl });
    }

    [HttpPost("settings/logo")]
    [RequirePermission(Permissions.Menu.Edit)]
    public async Task<IActionResult> UploadLogo(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Dosya seçilmedi." });

        var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowed.Contains(file.ContentType))
            return BadRequest(new { message = "Sadece JPEG, PNG veya WebP yüklenebilir." });

        if (file.Length > 2 * 1024 * 1024)
            return BadRequest(new { message = "Dosya boyutu 2MB'ı geçemez." });

        var tenantId = HttpContext.User.FindFirst("tenant_id")?.Value
            ?? HttpContext.User.FindFirst("tenantId")?.Value;

        if (string.IsNullOrEmpty(tenantId) || !Guid.TryParse(tenantId, out var tid))
            return Forbid();

        var tenant = await db.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == tid, ct);
        if (tenant == null) return NotFound();

        if (!string.IsNullOrEmpty(tenant.LogoUrl))
            await storage.DeleteAsync(tenant.LogoUrl, ct);

        using var stream = file.OpenReadStream();
        var logoUrl = await storage.UploadAsync(stream, file.FileName, file.ContentType, "logos", ct);

        tenant.LogoUrl = logoUrl;
        await db.SaveChangesAsync(ct);

        return Ok(new { logoUrl });
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken ct) =>
        Ok(await mediator.Send(new GetMenuCategoriesQuery(), ct));

    [HttpPost("categories")]
    [RequirePermission(Permissions.Menu.Edit)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateMenuCategoryCommand command, CancellationToken ct) =>
        Ok(await mediator.Send(command, ct));

    [HttpPatch("categories/{id:guid}")]
    [RequirePermission(Permissions.Menu.Edit)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest req, CancellationToken ct) =>
        Ok(await mediator.Send(new UpdateMenuCategoryCommand(id, req.Name, req.DisplayOrder, req.IsActive), ct));

    [HttpDelete("categories/{id:guid}")]
    [RequirePermission(Permissions.Menu.Edit)]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteMenuCategoryCommand(id), ct);
        return NoContent();
    }

    [HttpGet("items")]
    public async Task<IActionResult> GetItems([FromQuery] Guid? categoryId, CancellationToken ct) =>
        Ok(await mediator.Send(new GetMenuItemsQuery(categoryId), ct));

    [HttpPost("items")]
    [RequirePermission(Permissions.Menu.Edit)]
    public async Task<IActionResult> CreateItem([FromBody] CreateMenuItemCommand command, CancellationToken ct) =>
        Ok(await mediator.Send(command, ct));

    [HttpPatch("items/{id:guid}")]
    [RequirePermission(Permissions.Menu.Edit)]
    public async Task<IActionResult> UpdateItem(Guid id, [FromBody] UpdateItemRequest req, CancellationToken ct)
    {
        var ingredients = req.Ingredients?.Select(i => new IngredientRequest(i.StockItemId, i.Quantity)).ToList();
        return Ok(await mediator.Send(new UpdateMenuItemCommand(
            id, req.Name, req.Description, req.Price, req.CostPrice, req.CategoryId, req.IsAvailable, ingredients), ct));
    }

    [HttpPost("items/{id:guid}/image")]
    [RequirePermission(Permissions.Menu.Edit)]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Dosya seçilmedi." });

        var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowed.Contains(file.ContentType))
            return BadRequest(new { message = "Sadece JPEG, PNG veya WebP yüklenebilir." });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "Dosya boyutu 5MB'ı geçemez." });

        var item = await db.MenuItems.FirstOrDefaultAsync(i => i.Id == id, ct);
        if (item == null) return NotFound();

        // Eski resmi Supabase Storage'dan sil
        if (!string.IsNullOrEmpty(item.ImageUrl))
            await storage.DeleteAsync(item.ImageUrl, ct);

        using var stream = file.OpenReadStream();
        var imageUrl = await storage.UploadAsync(stream, file.FileName, file.ContentType, "items", ct);

        item.ImageUrl = imageUrl;
        await db.SaveChangesAsync(ct);

        return Ok(new { imageUrl });
    }

    [HttpDelete("items/{id:guid}")]
    [RequirePermission(Permissions.Menu.Edit)]
    public async Task<IActionResult> DeleteItem(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteMenuItemCommand(id), ct);
        return NoContent();
    }
}
