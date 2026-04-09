using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Memberships.DTOs;

namespace StockBite.Application.Memberships.Queries;

public record GetMembersQuery(string? Search) : IRequest<List<MemberDto>>;

public class GetMembersQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetMembersQuery, List<MemberDto>>
{
    public async Task<List<MemberDto>> Handle(GetMembersQuery request, CancellationToken ct)
    {
        var query = db.Members
            .Include(m => m.Subscriptions)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim().ToLower();
            query = query.Where(m => m.Name.ToLower().Contains(s) || (m.Phone != null && m.Phone.Contains(s)));
        }

        var members = await query
            .OrderBy(m => m.Name)
            .ToListAsync(ct);

        return members.Select(m => new MemberDto(
            m.Id,
            m.Name,
            m.Phone,
            m.Note,
            m.Subscriptions.Count,
            m.Subscriptions.Sum(s => s.RemainingHours),
            m.CreatedAt
        )).ToList();
    }
}
