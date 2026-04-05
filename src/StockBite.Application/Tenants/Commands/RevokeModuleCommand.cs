using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Enums;

namespace StockBite.Application.Tenants.Commands;

public record RevokeModuleCommand(Guid TenantId, ModuleType ModuleType) : IRequest;

public class RevokeModuleCommandHandler(IApplicationDbContext db)
    : IRequestHandler<RevokeModuleCommand>
{
    public async Task Handle(RevokeModuleCommand request, CancellationToken ct)
    {
        var module = await db.TenantModules
            .FirstOrDefaultAsync(tm => tm.TenantId == request.TenantId && tm.ModuleType == request.ModuleType, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.TenantModule), $"{request.TenantId}/{request.ModuleType}");
        module.IsActive = false;
        await db.SaveChangesAsync(ct);
    }
}
