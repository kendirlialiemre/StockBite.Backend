using MediatR;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Events.DTOs;
using StockBite.Domain.Enums;

namespace StockBite.Application.Events.Commands;

public record CreateEventCommand(
    string PersonName, int? Age, DateOnly EventDate,
    int AdultCount, int ChildCount, string EventType,
    string? Package, decimal ChargedAmount, decimal Cost,
    string? Notes) : IRequest<EventDto>;

public class CreateEventCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<CreateEventCommand, EventDto>
{
    public async Task<EventDto> Handle(CreateEventCommand request, CancellationToken ct)
    {
        var ev = new Domain.Entities.Event
        {
            TenantId = currentUser.TenantId!.Value,
            PersonName = request.PersonName,
            Age = request.Age,
            EventDate = request.EventDate,
            AdultCount = request.AdultCount,
            ChildCount = request.ChildCount,
            EventType = request.EventType,
            Package = request.Package,
            ChargedAmount = request.ChargedAmount,
            Cost = request.Cost,
            Notes = request.Notes,
            Status = EventStatus.Planned,
        };
        db.Events.Add(ev);
        await db.SaveChangesAsync(ct);
        return new EventDto(ev.Id, ev.PersonName, ev.Age, ev.EventDate,
            ev.AdultCount, ev.ChildCount, ev.EventType, ev.Package,
            ev.ChargedAmount, ev.Cost, ev.ChargedAmount - ev.Cost,
            ev.Notes, ev.Status, ev.CreatedAt,
            ev.PaymentMethod, ev.CashAmount, ev.CardAmount, ev.PaidAt);
    }
}
