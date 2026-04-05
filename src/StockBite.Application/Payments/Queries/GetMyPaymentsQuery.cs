using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Enums;

namespace StockBite.Application.Payments.Queries;

public record MyPaymentDto(
    Guid Id,
    string PackageName,
    string PackageDescription,
    decimal Amount,
    PaymentStatus Status,
    DateTime PurchasedAt,
    DateTime? ExpiresAt
);

public record GetMyPaymentsQuery : IRequest<List<MyPaymentDto>>;

public class GetMyPaymentsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GetMyPaymentsQuery, List<MyPaymentDto>>
{
    public async Task<List<MyPaymentDto>> Handle(GetMyPaymentsQuery request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId;
        if (tenantId == null) return [];

        var payments = await db.Payments
            .Include(p => p.Package).ThenInclude(p => p.Modules)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        if (payments.Count == 0) return [];

        var tenantModules = await db.TenantModules
            .Where(tm => tm.TenantId == tenantId)
            .ToListAsync(ct);

        return payments.Select(p =>
        {
            var packageModuleTypes = p.Package.Modules.Select(m => m.ModuleType).ToList();
            var expiresAt = tenantModules
                .Where(tm => packageModuleTypes.Contains(tm.ModuleType))
                .Select(tm => tm.ExpiresAt)
                .Where(e => e.HasValue)
                .OrderBy(e => e)
                .FirstOrDefault();

            return new MyPaymentDto(
                p.Id,
                p.Package.Name,
                p.Package.Description,
                p.Amount,
                p.Status,
                p.CreatedAt,
                expiresAt
            );
        }).ToList();
    }
}
