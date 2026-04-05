using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockBite.Application.Tenants.Commands;
using StockBite.Application.Tenants.Queries;
using StockBite.Domain.Enums;

namespace StockBite.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/tenants")]
[Authorize(Roles = "SuperAdmin")]
public class TenantsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, CancellationToken ct) =>
        Ok(await mediator.Send(new GetTenantsQuery(search), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) =>
        Ok(await mediator.Send(new GetTenantByIdQuery(id), ct));

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTenantStatusRequest req, CancellationToken ct)
    {
        await mediator.Send(new UpdateTenantStatusCommand(id, req.IsActive), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/modules")]
    public async Task<IActionResult> GetModules(Guid id, CancellationToken ct) =>
        Ok(await mediator.Send(new GetTenantModulesQuery(id), ct));

    [HttpPost("{id:guid}/modules")]
    public async Task<IActionResult> AssignModule(Guid id, [FromBody] AssignModuleRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new AssignModuleCommand(id, (ModuleType)req.ModuleId, req.GrantedByAdmin, req.ExpiresAt), ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/modules/{moduleId:int}")]
    public async Task<IActionResult> RevokeModule(Guid id, int moduleId, CancellationToken ct)
    {
        await mediator.Send(new RevokeModuleCommand(id, (ModuleType)moduleId), ct);
        return NoContent();
    }
}

public record UpdateTenantStatusRequest(bool IsActive);
public record AssignModuleRequest(int ModuleId, bool GrantedByAdmin = false, DateTime? ExpiresAt = null);
