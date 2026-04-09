using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockBite.Api.Filters;
using StockBite.Application.Memberships.Commands;
using StockBite.Application.Memberships.Queries;
#pragma warning disable CA2201
using StockBite.Domain.Enums;

namespace StockBite.Api.Controllers.Memberships;

[ApiController]
[Route("api/memberships")]
[Authorize]
[RequireModule(ModuleType.Memberships)]
public class MembershipsController(IMediator mediator) : ControllerBase
{
    [HttpGet("members")]
    public async Task<IActionResult> GetMembers([FromQuery] string? search, CancellationToken ct) =>
        Ok(await mediator.Send(new GetMembersQuery(search), ct));

    [HttpGet("members/{memberId:guid}")]
    public async Task<IActionResult> GetMemberDetail(Guid memberId, CancellationToken ct) =>
        Ok(await mediator.Send(new GetMemberDetailQuery(memberId), ct));

    [HttpPost("members")]
    public async Task<IActionResult> CreateMember([FromBody] CreateMemberRequest req, CancellationToken ct) =>
        Ok(await mediator.Send(new CreateMemberCommand(req.Name, req.Phone, req.Note), ct));

    [HttpDelete("members/{memberId:guid}")]
    public async Task<IActionResult> DeleteMember(Guid memberId, CancellationToken ct)
    {
        await mediator.Send(new DeleteMemberCommand(memberId), ct);
        return NoContent();
    }

    [HttpPost("subscriptions")]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionRequest req, CancellationToken ct) =>
        Ok(await mediator.Send(new CreateSubscriptionCommand(req.MemberId, req.TotalHours, req.Price, req.Note), ct));

    [HttpPut("subscriptions/{subscriptionId:guid}")]
    public async Task<IActionResult> UpdateSubscription(Guid subscriptionId, [FromBody] UpdateSubscriptionRequest req, CancellationToken ct) =>
        Ok(await mediator.Send(new UpdateSubscriptionCommand(subscriptionId, req.TotalHours, req.Price, req.Note), ct));

    [HttpDelete("subscriptions/{subscriptionId:guid}")]
    public async Task<IActionResult> DeleteSubscription(Guid subscriptionId, CancellationToken ct)
    {
        await mediator.Send(new DeleteSubscriptionCommand(subscriptionId), ct);
        return NoContent();
    }

    [HttpPost("sessions")]
    public async Task<IActionResult> RecordSession([FromBody] RecordSessionRequest req, CancellationToken ct) =>
        Ok(await mediator.Send(new RecordSessionCommand(req.SubscriptionId, req.Hours, req.Note), ct));

    [HttpPut("sessions/{sessionId:guid}")]
    public async Task<IActionResult> UpdateSession(Guid sessionId, [FromBody] UpdateSessionRequest req, CancellationToken ct) =>
        Ok(await mediator.Send(new UpdateSessionCommand(sessionId, req.Hours, req.Note), ct));

    [HttpDelete("sessions/{sessionId:guid}")]
    public async Task<IActionResult> DeleteSession(Guid sessionId, CancellationToken ct)
    {
        await mediator.Send(new DeleteSessionCommand(sessionId), ct);
        return NoContent();
    }
}

public record CreateMemberRequest(string Name, string? Phone, string? Note);
public record CreateSubscriptionRequest(Guid MemberId, decimal TotalHours, decimal Price, string? Note);
public record UpdateSubscriptionRequest(decimal TotalHours, decimal Price, string? Note);
public record RecordSessionRequest(Guid SubscriptionId, decimal Hours, string? Note);
public record UpdateSessionRequest(decimal Hours, string? Note);
