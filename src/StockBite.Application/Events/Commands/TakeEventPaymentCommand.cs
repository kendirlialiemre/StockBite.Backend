using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Events.DTOs;
using StockBite.Domain.Enums;

namespace StockBite.Application.Events.Commands;

public record TakeEventPaymentCommand(
    Guid Id,
    PaymentMethod Method,
    decimal CashAmount,
    decimal CardAmount) : IRequest<EventDto>;

public class TakeEventPaymentCommandHandler(IApplicationDbContext db)
    : IRequestHandler<TakeEventPaymentCommand, EventDto>
{
    public async Task<EventDto> Handle(TakeEventPaymentCommand request, CancellationToken ct)
    {
        var ev = await db.Events.FirstOrDefaultAsync(e => e.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Event), request.Id);

        ev.PaymentMethod = request.Method;
        ev.CashAmount = request.CashAmount;
        ev.CardAmount = request.CardAmount;
        ev.PaidAt = DateTime.UtcNow;
        ev.Status = EventStatus.Completed;

        await db.SaveChangesAsync(ct);

        return new EventDto(ev.Id, ev.PersonName, ev.Age, ev.EventDate,
            ev.AdultCount, ev.ChildCount, ev.EventType, ev.Package,
            ev.ChargedAmount, ev.Cost, ev.ChargedAmount - ev.Cost,
            ev.Notes, ev.Status, ev.CreatedAt,
            ev.PaymentMethod, ev.CashAmount, ev.CardAmount, ev.PaidAt);
    }
}
