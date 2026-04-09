using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Memberships.DTOs;
using StockBite.Domain.Entities;

namespace StockBite.Application.Memberships.Queries;

public record GetMemberDetailQuery(Guid MemberId) : IRequest<MemberDetailDto>;

public class GetMemberDetailQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetMemberDetailQuery, MemberDetailDto>
{
    public async Task<MemberDetailDto> Handle(GetMemberDetailQuery request, CancellationToken ct)
    {
        var member = await db.Members
            .Include(m => m.Subscriptions)
                .ThenInclude(s => s.Sessions)
            .FirstOrDefaultAsync(m => m.Id == request.MemberId, ct)
            ?? throw new NotFoundException(nameof(Member), request.MemberId);

        return new MemberDetailDto(
            member.Id,
            member.Name,
            member.Phone,
            member.Note,
            member.CreatedAt,
            member.Subscriptions
                .OrderByDescending(s => s.PurchasedAt)
                .Select(s => new SubscriptionDto(
                    s.Id,
                    s.TotalHours,
                    s.RemainingHours,
                    s.Price,
                    s.Note,
                    s.PurchasedAt,
                    s.Sessions
                        .OrderByDescending(ss => ss.SessionAt)
                        .Select(ss => new SessionDto(ss.Id, ss.Hours, ss.Note, ss.SessionAt))
                        .ToList()
                )).ToList()
        );
    }
}
