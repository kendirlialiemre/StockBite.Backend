using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Entities;

namespace StockBite.Application.Users.Commands;

public record UpdatePermissionsCommand(Guid UserId, IReadOnlyList<string> Permissions) : IRequest;

public class UpdatePermissionsCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<UpdatePermissionsCommand>
{
    public async Task Handle(UpdatePermissionsCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException();
        var grantedBy = currentUser.UserId ?? throw new ForbiddenException();

        var existing = await db.TenantUserPermissions
            .Where(p => p.TenantId == tenantId && p.UserId == request.UserId)
            .ToListAsync(ct);
        db.TenantUserPermissions.RemoveRange(existing);

        var newPermissions = request.Permissions.Select(p => new TenantUserPermission
        {
            TenantId = tenantId,
            UserId = request.UserId,
            Permission = p,
            GrantedBy = grantedBy
        });
        db.TenantUserPermissions.AddRange(newPermissions);
        await db.SaveChangesAsync(ct);
    }
}
