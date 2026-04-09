using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Memberships.DTOs;
using StockBite.Domain.Entities;

namespace StockBite.Application.Memberships.Commands;

public record CreateSubscriptionCommand(
    Guid MemberId,
    decimal TotalHours,
    decimal Price,
    string? Note
) : IRequest<SubscriptionDto>;

public class CreateSubscriptionCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<CreateSubscriptionCommand, SubscriptionDto>
{
    public async Task<SubscriptionDto> Handle(CreateSubscriptionCommand request, CancellationToken ct)
    {
        var member = await db.Members.FirstOrDefaultAsync(m => m.Id == request.MemberId, ct)
            ?? throw new NotFoundException(nameof(Member), request.MemberId);

        var sub = new MemberSubscription
        {
            TenantId = currentUser.TenantId!.Value,
            MemberId = member.Id,
            TotalHours = request.TotalHours,
            RemainingHours = request.TotalHours,
            Price = request.Price,
            Note = request.Note?.Trim(),
        };
        db.MemberSubscriptions.Add(sub);
        await db.SaveChangesAsync(ct);
        return new SubscriptionDto(sub.Id, sub.TotalHours, sub.RemainingHours, sub.Price, sub.Note, sub.PurchasedAt, []);
    }
}
