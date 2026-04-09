using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Memberships.DTOs;
using StockBite.Domain.Entities;
#pragma warning disable CA2201

namespace StockBite.Application.Memberships.Commands;

public record RecordSessionCommand(
    Guid SubscriptionId,
    decimal Hours,
    string? Note
) : IRequest<SessionDto>;

public class RecordSessionCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<RecordSessionCommand, SessionDto>
{
    public async Task<SessionDto> Handle(RecordSessionCommand request, CancellationToken ct)
    {
        var sub = await db.MemberSubscriptions
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId, ct)
            ?? throw new NotFoundException(nameof(MemberSubscription), request.SubscriptionId);

        if (request.Hours <= 0)
            throw new InvalidOperationException("Saat 0'dan büyük olmalı.");

        if (sub.RemainingHours < request.Hours)
            throw new InvalidOperationException($"Yeterli saat yok. Kalan: {sub.RemainingHours:0.##} saat.");

        sub.RemainingHours -= request.Hours;

        var session = new SubscriptionSession
        {
            TenantId = currentUser.TenantId!.Value,
            SubscriptionId = sub.Id,
            Hours = request.Hours,
            Note = request.Note?.Trim(),
        };
        db.SubscriptionSessions.Add(session);
        await db.SaveChangesAsync(ct);
        return new SessionDto(session.Id, session.Hours, session.Note, session.SessionAt);
    }
}
