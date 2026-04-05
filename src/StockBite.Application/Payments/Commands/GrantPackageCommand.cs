using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Entities;
using StockBite.Domain.Enums;

namespace StockBite.Application.Payments.Commands;

public record GrantPackageCommand(Guid TenantId, Guid PackageId, DateTime? ExpiresAt) : IRequest;

public class GrantPackageCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GrantPackageCommand>
{
    public async Task Handle(GrantPackageCommand request, CancellationToken ct)
    {
        var package = await db.Packages.IgnoreQueryFilters()
            .Include(p => p.Modules)
            .FirstOrDefaultAsync(p => p.Id == request.PackageId, ct)
            ?? throw new NotFoundException(nameof(Package), request.PackageId);

        var adminUser = await db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == currentUser.UserId, ct)
            ?? throw new NotFoundException(nameof(User), currentUser.UserId!);

        // Calculate ExpiresAt: manual override > extend existing > from now
        DateTime? expiresAt;
        if (request.ExpiresAt.HasValue)
        {
            expiresAt = request.ExpiresAt;
        }
        else if (package.DurationDays.HasValue)
        {
            var moduleTypes = package.Modules.Select(m => m.ModuleType).ToList();
            var maxExisting = await db.TenantModules.IgnoreQueryFilters()
                .Where(tm => tm.TenantId == request.TenantId
                          && moduleTypes.Contains(tm.ModuleType)
                          && tm.IsActive
                          && tm.ExpiresAt.HasValue
                          && tm.ExpiresAt > DateTime.UtcNow)
                .Select(tm => tm.ExpiresAt!.Value)
                .OrderByDescending(e => e)
                .FirstOrDefaultAsync(ct);

            var baseDate = maxExisting > DateTime.UtcNow ? maxExisting : DateTime.UtcNow;
            expiresAt = baseDate.AddDays(package.DurationDays.Value);
        }
        else
        {
            expiresAt = null;
        }

        // Create a free payment record
        db.Payments.Add(new Payment
        {
            TenantId = request.TenantId,
            PackageId = package.Id,
            UserId = adminUser.Id,
            Amount = 0,
            Status = PaymentStatus.Free,
            ConversationId = Guid.NewGuid().ToString("N")
        });

        // Activate modules
        foreach (var module in package.Modules)
        {
            var existing = await db.TenantModules.IgnoreQueryFilters()
                .FirstOrDefaultAsync(tm => tm.TenantId == request.TenantId && tm.ModuleType == module.ModuleType, ct);

            if (existing != null)
            {
                existing.IsActive = true;
                existing.StartsAt = DateTime.UtcNow;
                existing.GrantedByAdmin = true;
                existing.ExpiresAt = expiresAt;
            }
            else
            {
                db.TenantModules.Add(new TenantModule
                {
                    TenantId = request.TenantId,
                    ModuleType = module.ModuleType,
                    IsActive = true,
                    GrantedByAdmin = true,
                    StartsAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt
                });
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
