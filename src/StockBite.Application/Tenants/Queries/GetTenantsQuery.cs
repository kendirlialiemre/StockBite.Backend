using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Tenants.DTOs;

namespace StockBite.Application.Tenants.Queries;

public record GetTenantsQuery(string? Search = null) : IRequest<List<TenantDto>>;

public class GetTenantsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetTenantsQuery, List<TenantDto>>
{
    public async Task<List<TenantDto>> Handle(GetTenantsQuery request, CancellationToken ct)
    {
        var query = db.Tenants.AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(t => t.Name.Contains(request.Search) || t.Slug.Contains(request.Search));

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TenantDto(t.Id, t.Name, t.Slug, t.IsActive, t.CreatedAt))
            .ToListAsync(ct);
    }
}
