using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Entities;

namespace StockBite.Application.Memberships.Commands;

public record DeleteSessionCommand(Guid SessionId) : IRequest;

public class DeleteSessionCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteSessionCommand>
{
    public async Task Handle(DeleteSessionCommand request, CancellationToken ct)
    {
        var session = await db.SubscriptionSessions
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, ct)
            ?? throw new NotFoundException(nameof(SubscriptionSession), request.SessionId);

        var sub = await db.MemberSubscriptions
            .FirstOrDefaultAsync(s => s.Id == session.SubscriptionId, ct)
            ?? throw new NotFoundException(nameof(MemberSubscription), session.SubscriptionId);

        // Silinen seans saatlerini geri ekle
        sub.RemainingHours += session.Hours;

        db.SubscriptionSessions.Remove(session);
        await db.SaveChangesAsync(ct);
    }
}
