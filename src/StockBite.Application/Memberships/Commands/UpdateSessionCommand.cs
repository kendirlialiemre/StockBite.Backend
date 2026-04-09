using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Memberships.DTOs;
using StockBite.Domain.Entities;

namespace StockBite.Application.Memberships.Commands;

public record UpdateSessionCommand(
    Guid SessionId,
    decimal Hours,
    string? Note
) : IRequest<SessionDto>;

public class UpdateSessionCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateSessionCommand, SessionDto>
{
    public async Task<SessionDto> Handle(UpdateSessionCommand request, CancellationToken ct)
    {
        var session = await db.SubscriptionSessions
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, ct)
            ?? throw new NotFoundException(nameof(SubscriptionSession), request.SessionId);

        var sub = await db.MemberSubscriptions
            .Include(s => s.Sessions)
            .FirstOrDefaultAsync(s => s.Id == session.SubscriptionId, ct)
            ?? throw new NotFoundException(nameof(MemberSubscription), session.SubscriptionId);

        if (request.Hours <= 0)
            throw new InvalidOperationException("Saat 0'dan büyük olmalı.");

        var usedHoursExcludingThis = sub.Sessions
            .Where(s => s.Id != session.Id)
            .Sum(s => s.Hours);

        if (usedHoursExcludingThis + request.Hours > sub.TotalHours)
            throw new InvalidOperationException($"Toplam saati ({sub.TotalHours:0.##}) aşıyor.");

        session.Hours = request.Hours;
        session.Note = request.Note?.Trim();
        sub.RemainingHours = sub.TotalHours - (usedHoursExcludingThis + request.Hours);

        await db.SaveChangesAsync(ct);
        return new SessionDto(session.Id, session.Hours, session.Note, session.SessionAt);
    }
}
