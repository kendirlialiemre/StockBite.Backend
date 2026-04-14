using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockBite.Api.Filters;
using StockBite.Application.Orders.Commands;
using StockBite.Application.Orders.Queries;
using StockBite.Domain.Constants;
using StockBite.Domain.Enums;

namespace StockBite.Api.Controllers.Orders;

[ApiController]
[Route("api/orders")]
[Authorize]
[RequireModule(ModuleType.Tables)]
public class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.Tables.View)]
    public async Task<IActionResult> GetOrders([FromQuery] int? status, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct) =>
        Ok(await mediator.Send(new GetOrdersQuery(status.HasValue ? (OrderStatus)status.Value : null, from, to), ct));

    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.Tables.View)]
    public async Task<IActionResult> GetOrderById(Guid id, CancellationToken ct) =>
        Ok(await mediator.Send(new GetOrderByIdQuery(id), ct));

    [HttpPost]
    [RequirePermission(Permissions.Tables.Manage)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command, CancellationToken ct) =>
        Ok(await mediator.Send(command, ct));

    [HttpPost("{orderId:guid}/items")]
    [RequirePermission(Permissions.Tables.Manage)]
    public async Task<IActionResult> AddItem(Guid orderId, [FromBody] AddItemRequest req, CancellationToken ct) =>
        Ok(await mediator.Send(new AddOrderItemCommand(orderId, req.MenuItemId, req.Quantity, req.Note), ct));

    [HttpDelete("{orderId:guid}/items/{itemId:guid}")]
    [RequirePermission(Permissions.Tables.Manage)]
    public async Task<IActionResult> RemoveItem(Guid orderId, Guid itemId, CancellationToken ct) =>
        Ok(await mediator.Send(new RemoveOrderItemCommand(orderId, itemId), ct));

    [HttpPost("{orderId:guid}/close")]
    [RequirePermission(Permissions.Tables.Manage)]
    public async Task<IActionResult> CloseOrder(Guid orderId, [FromBody] CloseOrderRequest req, CancellationToken ct)
    {
        await mediator.Send(new CloseOrderCommand(orderId, req.PaymentMethod, req.CashAmount, req.CardAmount), ct);
        return NoContent();
    }

    [HttpPost("{orderId:guid}/cancel")]
    [RequirePermission(Permissions.Tables.Manage)]
    public async Task<IActionResult> CancelOrder(Guid orderId, CancellationToken ct)
    {
        await mediator.Send(new CancelOrderCommand(orderId), ct);
        return NoContent();
    }

    [HttpPost("{orderId:guid}/timer/pause")]
    [RequirePermission(Permissions.Tables.Manage)]
    public async Task<IActionResult> PauseTimer(Guid orderId, CancellationToken ct)
    {
        await mediator.Send(new PauseTimerCommand(orderId), ct);
        return NoContent();
    }

    [HttpPost("{orderId:guid}/timer/resume")]
    [RequirePermission(Permissions.Tables.Manage)]
    public async Task<IActionResult> ResumeTimer(Guid orderId, CancellationToken ct)
    {
        await mediator.Send(new ResumeTimerCommand(orderId), ct);
        return NoContent();
    }
}

[ApiController]
[Route("api/tables")]
[Authorize]
[RequireModule(ModuleType.Tables)]
public class TablesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.Tables.View)]
    public async Task<IActionResult> GetTables(CancellationToken ct) =>
        Ok(await mediator.Send(new GetTablesQuery(), ct));

    [HttpGet("{id:guid}/order")]
    [RequirePermission(Permissions.Tables.View)]
    public async Task<IActionResult> GetTableOrder(Guid id, CancellationToken ct) =>
        Ok(await mediator.Send(new GetTableActiveOrderQuery(id), ct));

    [HttpPost]
    [RequirePermission(Permissions.Tables.Manage)]
    public async Task<IActionResult> OpenTable([FromBody] OpenTableRequest req, CancellationToken ct) =>
        Ok(await mediator.Send(new OpenTableCommand(req.Name), ct));

    [HttpPatch("{tableId:guid}/name")]
    [RequirePermission(Permissions.Tables.Manage)]
    public async Task<IActionResult> RenameTable(Guid tableId, [FromBody] RenameTableRequest req, CancellationToken ct)
    {
        await mediator.Send(new RenameTableCommand(tableId, req.Name), ct);
        return NoContent();
    }
}

public record AddItemRequest(Guid MenuItemId, int Quantity, string? Note);
public record OpenTableRequest(string Name);
public record RenameTableRequest(string Name);
public record CloseOrderRequest(PaymentMethod PaymentMethod, decimal? CashAmount = null, decimal? CardAmount = null);
