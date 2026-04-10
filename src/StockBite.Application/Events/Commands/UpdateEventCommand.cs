using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Events.DTOs;
using StockBite.Domain.Enums;

namespace StockBite.Application.Events.Commands;

public record UpdateEventCommand(
    Guid Id, string PersonName, int? Age, DateOnly EventDate,
    int AdultCount, int ChildCount, string EventType,
    string? Package, decimal ChargedAmount, decimal Cost,
    string? Notes, EventStatus Status) : IRequest<EventDto>;

public class UpdateEventCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateEventCommand, EventDto>
{
    public async Task<EventDto> Handle(UpdateEventCommand request, CancellationToken ct)
    {
        var ev = await db.Events.FirstOrDefaultAsync(e => e.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Event), request.Id);

        ev.PersonName = request.PersonName;
        ev.Age = request.Age;
        ev.EventDate = request.EventDate;
        ev.AdultCount = request.AdultCount;
        ev.ChildCount = request.ChildCount;
        ev.EventType = request.EventType;
        ev.Package = request.Package;
        ev.ChargedAmount = request.ChargedAmount;
        ev.Cost = request.Cost;
        ev.Notes = request.Notes;
        ev.Status = request.Status;

        await db.SaveChangesAsync(ct);
        return new EventDto(ev.Id, ev.PersonName, ev.Age, ev.EventDate,
            ev.AdultCount, ev.ChildCount, ev.EventType, ev.Package,
            ev.ChargedAmount, ev.Cost, ev.ChargedAmount - ev.Cost,
            ev.Notes, ev.Status, ev.CreatedAt,
            ev.PaymentMethod, ev.CashAmount, ev.CardAmount, ev.PaidAt);
    }
}
