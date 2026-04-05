using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Tenants.DTOs;
using StockBite.Domain.Entities;
using StockBite.Domain.Enums;

namespace StockBite.Application.Tenants.Commands;

public record AssignModuleCommand(Guid TenantId, ModuleType ModuleType, bool GrantedByAdmin, DateTime? ExpiresAt) : IRequest<TenantModuleDto>;

public class AssignModuleCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AssignModuleCommand, TenantModuleDto>
{
    public async Task<TenantModuleDto> Handle(AssignModuleCommand request, CancellationToken ct)
    {
        if (!await db.Tenants.AnyAsync(t => t.Id == request.TenantId, ct))
            throw new NotFoundException(nameof(Tenant), request.TenantId);

        var existing = await db.TenantModules
            .FirstOrDefaultAsync(tm => tm.TenantId == request.TenantId && tm.ModuleType == request.ModuleType, ct);

        if (existing != null)
        {
            existing.IsActive = true;
            existing.GrantedByAdmin = request.GrantedByAdmin;
            existing.ExpiresAt = request.ExpiresAt;
            existing.StartsAt = DateTime.UtcNow;
        }
        else
        {
            existing = new TenantModule
            {
                TenantId = request.TenantId,
                ModuleType = request.ModuleType,
                IsActive = true,
                GrantedByAdmin = request.GrantedByAdmin,
                StartsAt = DateTime.UtcNow,
                ExpiresAt = request.ExpiresAt
            };
            db.TenantModules.Add(existing);
        }

        await db.SaveChangesAsync(ct);
        return new TenantModuleDto((int)existing.ModuleType, existing.ModuleType.ToString(),
            existing.IsActive, existing.GrantedByAdmin, existing.StartsAt, existing.ExpiresAt);
    }
}
