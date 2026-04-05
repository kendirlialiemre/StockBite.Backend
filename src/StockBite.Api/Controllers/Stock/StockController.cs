using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockBite.Api.Filters;
using StockBite.Application.Stock.Commands;
using StockBite.Application.Stock.Queries;
using StockBite.Domain.Constants;
using StockBite.Domain.Enums;

namespace StockBite.Api.Controllers.Stock;

public record UpdateStockItemRequest(string? Name, string? Unit, decimal? LowStockThreshold);

[ApiController]
[Route("api/stock")]
[Authorize]
[RequireModule(ModuleType.Stock)]
public class StockController(IMediator mediator) : ControllerBase
{
    [HttpGet("items")]
    [RequirePermission(Permissions.Stock.View)]
    public async Task<IActionResult> GetItems(CancellationToken ct) =>
        Ok(await mediator.Send(new GetStockItemsQuery(), ct));

    [HttpPost("items")]
    [RequirePermission(Permissions.Stock.AddItem)]
    public async Task<IActionResult> CreateItem([FromBody] CreateStockItemCommand command, CancellationToken ct) =>
        Ok(await mediator.Send(command, ct));

    [HttpPatch("items/{id:guid}")]
    [RequirePermission(Permissions.Stock.EditItem)]
    public async Task<IActionResult> UpdateItem(Guid id, [FromBody] UpdateStockItemRequest req, CancellationToken ct) =>
        Ok(await mediator.Send(new UpdateStockItemCommand(id, req.Name, req.Unit, req.LowStockThreshold), ct));

    [HttpDelete("items/{id:guid}")]
    [RequirePermission(Permissions.Stock.EditItem)]
    public async Task<IActionResult> DeleteItem(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteStockItemCommand(id), ct);
        return NoContent();
    }

    [HttpGet("movements")]
    [RequirePermission(Permissions.Stock.View)]
    public async Task<IActionResult> GetMovements([FromQuery] Guid? stockItemId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default) =>
        Ok(await mediator.Send(new GetStockMovementsQuery(stockItemId, page, pageSize), ct));

    [HttpPost("movements")]
    [RequirePermission(Permissions.Stock.AddMovement)]
    public async Task<IActionResult> CreateMovement([FromBody] CreateStockMovementCommand command, CancellationToken ct) =>
        Ok(await mediator.Send(command, ct));
}
