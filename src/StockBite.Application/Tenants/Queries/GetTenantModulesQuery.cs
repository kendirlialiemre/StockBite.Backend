using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Tenants.DTOs;

namespace StockBite.Application.Tenants.Queries;

public record GetTenantModulesQuery(Guid TenantId) : IRequest<List<TenantModuleDto>>;

public class GetTenantModulesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetTenantModulesQuery, List<TenantModuleDto>>
{
    public async Task<List<TenantModuleDto>> Handle(GetTenantModulesQuery request, CancellationToken ct)
    {
        return await db.TenantModules
            .Where(tm => tm.TenantId == request.TenantId)
            .Select(tm => new TenantModuleDto(
                (int)tm.ModuleType, tm.ModuleType.ToString(),
                tm.IsActive, tm.GrantedByAdmin, tm.StartsAt, tm.ExpiresAt))
            .ToListAsync(ct);
    }
}
