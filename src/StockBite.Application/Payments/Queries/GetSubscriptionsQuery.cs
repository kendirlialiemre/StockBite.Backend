using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Enums;

namespace StockBite.Application.Payments.Queries;

public record SubscriptionDto(
    Guid Id,
    Guid TenantId,
    string TenantName,
    string UserName,
    string UserEmail,
    string PackageName,
    decimal Amount,
    PaymentStatus Status,
    DateTime PurchasedAt,
    DateTime? ExpiresAt
);

public record GetSubscriptionsQuery(Guid? TenantId = null) : IRequest<List<SubscriptionDto>>;

public class GetSubscriptionsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetSubscriptionsQuery, List<SubscriptionDto>>
{
    public async Task<List<SubscriptionDto>> Handle(GetSubscriptionsQuery request, CancellationToken ct)
    {
        var query = db.Payments
            .IgnoreQueryFilters()
            .Include(p => p.Package).ThenInclude(p => p.Modules)
            .AsQueryable();

        if (request.TenantId.HasValue)
            query = query.Where(p => p.TenantId == request.TenantId.Value);

        var payments = await query.OrderByDescending(p => p.CreatedAt).ToListAsync(ct);

        if (payments.Count == 0) return [];

        var tenantIds = payments.Select(p => p.TenantId).Distinct().ToList();
        var userIds = payments.Select(p => p.UserId).Distinct().ToList();

        var tenants = await db.Tenants.IgnoreQueryFilters()
            .Where(t => tenantIds.Contains(t.Id))
            .Select(t => new { t.Id, t.Name })
            .ToListAsync(ct);

        var users = await db.Users.IgnoreQueryFilters()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName, u.Email })
            .ToListAsync(ct);

        // Get ExpiresAt: take earliest expiry among modules activated for this tenant by this package
        var tenantModules = await db.TenantModules.IgnoreQueryFilters()
            .Where(tm => tenantIds.Contains(tm.TenantId))
            .ToListAsync(ct);

        return payments.Select(p =>
        {
            var tenant = tenants.FirstOrDefault(t => t.Id == p.TenantId);
            var user = users.FirstOrDefault(u => u.Id == p.UserId);
            var packageModuleTypes = p.Package.Modules.Select(m => m.ModuleType).ToList();
            var expiresAt = tenantModules
                .Where(tm => tm.TenantId == p.TenantId && packageModuleTypes.Contains(tm.ModuleType))
                .Select(tm => tm.ExpiresAt)
                .Where(e => e.HasValue)
                .OrderBy(e => e)
                .FirstOrDefault();

            return new SubscriptionDto(
                p.Id,
                p.TenantId,
                tenant?.Name ?? "-",
                user?.FullName ?? "-",
                user?.Email ?? "-",
                p.Package.Name,
                p.Amount,
                p.Status,
                p.CreatedAt,
                expiresAt
            );
        }).ToList();
    }
}
