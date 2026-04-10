using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Events.DTOs;

namespace StockBite.Application.Events.Queries;

public record GetEventsQuery(int? Year, int? Month) : IRequest<List<EventDto>>;

public class GetEventsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetEventsQuery, List<EventDto>>
{
    public async Task<List<EventDto>> Handle(GetEventsQuery request, CancellationToken ct)
    {
        var query = db.Events.AsQueryable();

        if (request.Year.HasValue)
            query = query.Where(e => e.EventDate.Year == request.Year.Value);

        if (request.Month.HasValue)
            query = query.Where(e => e.EventDate.Month == request.Month.Value);

        return await query
            .OrderBy(e => e.EventDate)
            .Select(e => new EventDto(
                e.Id, e.PersonName, e.Age, e.EventDate,
                e.AdultCount, e.ChildCount, e.EventType, e.Package,
                e.ChargedAmount, e.Cost, e.ChargedAmount - e.Cost,
                e.Notes, e.Status, e.CreatedAt,
                e.PaymentMethod, e.CashAmount, e.CardAmount, e.PaidAt))
            .ToListAsync(ct);
    }
}
