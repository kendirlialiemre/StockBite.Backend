using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockBite.Application.Payments.Commands;
using StockBite.Application.Payments.Queries;

namespace StockBite.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/subscriptions")]
[Authorize(Roles = "SuperAdmin")]
public class SubscriptionsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? tenantId, CancellationToken ct) =>
        Ok(await mediator.Send(new GetSubscriptionsQuery(tenantId), ct));

    [HttpGet("check-conflict")]
    public async Task<IActionResult> CheckConflict([FromQuery] Guid tenantId, [FromQuery] Guid packageId, CancellationToken ct) =>
        Ok(await mediator.Send(new CheckPackageConflictQuery(tenantId, packageId), ct));

    [HttpPost("grant")]
    public async Task<IActionResult> Grant([FromBody] GrantPackageRequest req, CancellationToken ct)
    {
        await mediator.Send(new GrantPackageCommand(req.TenantId, req.PackageId, req.ExpiresAt), ct);
        return NoContent();
    }
}

public record GrantPackageRequest(Guid TenantId, Guid PackageId, DateTime? ExpiresAt);
