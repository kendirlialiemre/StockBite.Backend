using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Entities;

namespace StockBite.Application.Memberships.Commands;

public record DeleteSubscriptionCommand(Guid SubscriptionId) : IRequest;

public class DeleteSubscriptionCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteSubscriptionCommand>
{
    public async Task Handle(DeleteSubscriptionCommand request, CancellationToken ct)
    {
        var sub = await db.MemberSubscriptions
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId, ct)
            ?? throw new NotFoundException(nameof(MemberSubscription), request.SubscriptionId);

        db.MemberSubscriptions.Remove(sub);
        await db.SaveChangesAsync(ct);
    }
}
