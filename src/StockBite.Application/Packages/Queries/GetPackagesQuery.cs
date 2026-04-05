using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Packages.DTOs;

namespace StockBite.Application.Packages.Queries;

public record GetPackagesQuery(bool ActiveOnly = true) : IRequest<List<PackageDto>>;

public class GetPackagesQueryHandler(IApplicationDbContext db) : IRequestHandler<GetPackagesQuery, List<PackageDto>>
{
    public async Task<List<PackageDto>> Handle(GetPackagesQuery request, CancellationToken ct)
    {
        var query = db.Packages.Include(p => p.Modules).AsQueryable();
        if (request.ActiveOnly) query = query.Where(p => p.IsActive);
        return await query.OrderBy(p => p.Price).Select(p => new PackageDto(
            p.Id, p.Name, p.Description, p.Price, p.DurationDays, p.IsActive,
            p.Modules.Select(m => new PackageModuleDto((int)m.ModuleType, m.ModuleType.ToString())).ToList()
        )).ToListAsync(ct);
    }
}
