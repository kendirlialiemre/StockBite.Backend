using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Memberships.DTOs;
using StockBite.Domain.Entities;

namespace StockBite.Application.Memberships.Commands;

public record UpdateSubscriptionCommand(
    Guid SubscriptionId,
    decimal TotalHours,
    decimal Price,
    string? Note
) : IRequest<SubscriptionDto>;

public class UpdateSubscriptionCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateSubscriptionCommand, SubscriptionDto>
{
    public async Task<SubscriptionDto> Handle(UpdateSubscriptionCommand request, CancellationToken ct)
    {
        var sub = await db.MemberSubscriptions
            .Include(s => s.Sessions)
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId, ct)
            ?? throw new NotFoundException(nameof(MemberSubscription), request.SubscriptionId);

        var usedHours = sub.Sessions.Sum(s => s.Hours);

        if (request.TotalHours < usedHours)
            throw new InvalidOperationException($"Toplam saat kullanılan saatten ({usedHours:0.##}) az olamaz.");

        sub.TotalHours = request.TotalHours;
        sub.RemainingHours = request.TotalHours - usedHours;
        sub.Price = request.Price;
        sub.Note = request.Note?.Trim();

        await db.SaveChangesAsync(ct);

        var sessions = sub.Sessions
            .OrderByDescending(s => s.SessionAt)
            .Select(s => new SessionDto(s.Id, s.Hours, s.Note, s.SessionAt))
            .ToList();

        return new SubscriptionDto(sub.Id, sub.TotalHours, sub.RemainingHours, sub.Price, sub.Note, sub.PurchasedAt, sessions);
    }
}
