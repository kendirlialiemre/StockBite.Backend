using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using StockBite.Api.Filters;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Menu.Commands;
using StockBite.Application.Menu.Queries;
using StockBite.Domain.Enums;

namespace StockBite.Api.Controllers.Menu;

public record CreateQrCodeRequest(string Label, string BaseUrl);

[ApiController]
[Route("api/menu/qr-codes")]
[Authorize]
[RequireModule(ModuleType.Menu)]
public class MenuQrCodesController(
    IMediator mediator,
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IStorageService storage) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await mediator.Send(new GetMenuQrCodesQuery(), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQrCodeRequest req, CancellationToken ct)
    {
        var tenant = await db.Tenants
            .FirstOrDefaultAsync(t => t.Id == currentUser.TenantId, ct);
        if (tenant == null) return Unauthorized();

        var id = Guid.NewGuid();
        var publicUrl = $"{req.BaseUrl.TrimEnd('/')}/m/{tenant.Slug}?ref={id}";
        var filePath = await GenerateAndUploadQrAsync(id, publicUrl, ct);

        var dto = await mediator.Send(
            new CreateMenuQrCodeCommand(id, tenant.Id, req.Label, filePath, publicUrl), ct);

        return Ok(dto);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var filePath = await mediator.Send(new DeleteMenuQrCodeCommand(id), ct);
        if (!string.IsNullOrEmpty(filePath))
            await storage.DeleteAsync(filePath, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/regenerate")]
    public async Task<IActionResult> Regenerate(Guid id, [FromBody] RegenerateQrCodeRequest req, CancellationToken ct)
    {
        var tenant = await db.Tenants
            .FirstOrDefaultAsync(t => t.Id == currentUser.TenantId, ct);
        if (tenant == null) return Unauthorized();

        var newPublicUrl = $"{req.BaseUrl.TrimEnd('/')}/m/{tenant.Slug}?ref={id}";
        var newFilePath = await GenerateAndUploadQrAsync(id, newPublicUrl, ct);

        var (dto, oldFilePath) = await mediator.Send(
            new RegenerateMenuQrCodeCommand(id, newFilePath, newPublicUrl), ct);

        if (!string.IsNullOrEmpty(oldFilePath))
            await storage.DeleteAsync(oldFilePath, ct);

        return Ok(dto);
    }

    private async Task<string> GenerateAndUploadQrAsync(Guid id, string url, CancellationToken ct)
    {
        using var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrData);
        var pngBytes = qrCode.GetGraphic(20);

        using var ms = new MemoryStream(pngBytes);
        return await storage.UploadAsync(ms, $"{id}.png", "image/png", "qr", ct);
    }
}

public record RegenerateQrCodeRequest(string BaseUrl);
