using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;

namespace StockBite.Api.Controllers;

[ApiController]
[Route("api/me")]
[Authorize]
public class MeController(IApplicationDbContext db, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet("info")]
    public async Task<IActionResult> GetMyInfo(CancellationToken ct)
    {
        var tenantId = currentUser.TenantId;
        if (tenantId == null) return Unauthorized();

        var tenant = await db.Tenants
            .IgnoreQueryFilters()
            .Where(t => t.Id == tenantId)
            .Select(t => new { t.Slug, t.Name, t.QrMenuTemplate })
            .FirstOrDefaultAsync(ct);

        if (tenant == null) return NotFound();
        return Ok(tenant);
    }

    // Tenant'ın aktif modül ID'lerini döner — customer app sidebar için kullanır
    [HttpGet("modules")]
    public async Task<IActionResult> GetMyModules(CancellationToken ct)
    {
        var tenantId = currentUser.TenantId;
        if (tenantId == null) return Ok(Array.Empty<int>());

        var modules = await db.TenantModules
            .Where(tm => tm.TenantId == tenantId
                      && tm.IsActive
                      && (tm.ExpiresAt == null || tm.ExpiresAt > DateTime.UtcNow))
            .Select(tm => (int)tm.ModuleType)
            .ToListAsync(ct);

        return Ok(modules);
    }
}
