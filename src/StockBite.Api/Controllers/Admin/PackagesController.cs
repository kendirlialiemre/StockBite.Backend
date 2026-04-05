using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockBite.Application.Packages.Commands;
using StockBite.Application.Packages.Queries;

namespace StockBite.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/packages")]
[Authorize(Roles = "SuperAdmin")]
public class PackagesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await mediator.Send(new GetPackagesQuery(ActiveOnly: false), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePackageCommand command, CancellationToken ct) =>
        Ok(await mediator.Send(command, ct));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePackageRequest req, CancellationToken ct) =>
        Ok(await mediator.Send(new UpdatePackageCommand(id, req.Name, req.Description, req.Price, req.DurationDays, req.IsActive, req.ModuleTypes), ct));
}

public class UpdatePackageRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int? DurationDays { get; set; }
    public bool IsActive { get; set; }
    public List<int> ModuleTypes { get; set; } = [];
}
