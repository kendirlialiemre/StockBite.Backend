using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockBite.Api.Filters;
using StockBite.Application.Events.Commands;
using StockBite.Application.Events.Queries;
using StockBite.Domain.Constants;
using StockBite.Domain.Enums;

namespace StockBite.Api.Controllers.Events;

public record CreateEventRequest(
    string PersonName, int? Age, DateOnly EventDate,
    int AdultCount, int ChildCount, string EventType,
    string? Package, decimal ChargedAmount, decimal Cost, string? Notes);

public record TakePaymentRequest(int Method, decimal CashAmount, decimal CardAmount);

public record UpdateEventRequest(
    string PersonName, int? Age, DateOnly EventDate,
    int AdultCount, int ChildCount, string EventType,
    string? Package, decimal ChargedAmount, decimal Cost,
    string? Notes, EventStatus Status);

[ApiController]
[Route("api/events")]
[Authorize]
[RequireModule(ModuleType.Events)]
public class EventsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.Events.View)]
    public async Task<IActionResult> GetAll([FromQuery] int? year, [FromQuery] int? month, CancellationToken ct) =>
        Ok(await mediator.Send(new GetEventsQuery(year, month), ct));

    [HttpPost]
    [RequirePermission(Permissions.Events.Manage)]
    public async Task<IActionResult> Create([FromBody] CreateEventRequest req, CancellationToken ct) =>
        Ok(await mediator.Send(new CreateEventCommand(
            req.PersonName, req.Age, req.EventDate,
            req.AdultCount, req.ChildCount, req.EventType,
            req.Package, req.ChargedAmount, req.Cost, req.Notes), ct));

    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.Events.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventRequest req, CancellationToken ct) =>
        Ok(await mediator.Send(new UpdateEventCommand(
            id, req.PersonName, req.Age, req.EventDate,
            req.AdultCount, req.ChildCount, req.EventType,
            req.Package, req.ChargedAmount, req.Cost,
            req.Notes, req.Status), ct));

    [HttpPost("{id:guid}/payment")]
    [RequirePermission(Permissions.Events.Manage)]
    public async Task<IActionResult> TakePayment(Guid id, [FromBody] TakePaymentRequest req, CancellationToken ct) =>
        Ok(await mediator.Send(new TakeEventPaymentCommand(
            id, (StockBite.Domain.Enums.PaymentMethod)req.Method, req.CashAmount, req.CardAmount), ct));

    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.Events.Manage)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteEventCommand(id), ct);
        return NoContent();
    }
}
