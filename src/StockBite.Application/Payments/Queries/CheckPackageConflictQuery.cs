using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;

namespace StockBite.Application.Payments.Queries;

public record PackageConflictResult(
    bool HasConflict,
    DateTime? CurrentExpiresAt,
    DateTime? SuggestedNewExpiresAt
);

public record CheckPackageConflictQuery(Guid TenantId, Guid PackageId) : IRequest<PackageConflictResult>;

public class CheckPackageConflictQueryHandler(IApplicationDbContext db)
    : IRequestHandler<CheckPackageConflictQuery, PackageConflictResult>
{
    public async Task<PackageConflictResult> Handle(CheckPackageConflictQuery request, CancellationToken ct)
    {
        var package = await db.Packages.IgnoreQueryFilters()
            .Include(p => p.Modules)
            .FirstOrDefaultAsync(p => p.Id == request.PackageId, ct);

        if (package == null)
            return new PackageConflictResult(false, null, null);

        var moduleTypes = package.Modules.Select(m => m.ModuleType).ToList();

        var existingModules = await db.TenantModules.IgnoreQueryFilters()
            .Where(tm => tm.TenantId == request.TenantId
                      && moduleTypes.Contains(tm.ModuleType)
                      && tm.IsActive
                      && (tm.ExpiresAt == null || tm.ExpiresAt > DateTime.UtcNow))
            .ToListAsync(ct);

        if (!existingModules.Any())
            return new PackageConflictResult(false, null, null);

        // Take the latest expiry among overlapping active modules
        var maxExpiry = existingModules
            .Where(m => m.ExpiresAt.HasValue)
            .Select(m => m.ExpiresAt!.Value)
            .DefaultIfEmpty(DateTime.UtcNow)
            .Max();

        // If any module is unlimited (null ExpiresAt), it's an unlimited conflict
        var hasUnlimited = existingModules.Any(m => m.ExpiresAt == null);
        if (hasUnlimited)
            return new PackageConflictResult(true, null, null);

        var suggested = package.DurationDays.HasValue
            ? maxExpiry.AddDays(package.DurationDays.Value)
            : (DateTime?)null;

        return new PackageConflictResult(true, maxExpiry, suggested);
    }
}
