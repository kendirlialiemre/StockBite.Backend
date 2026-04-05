using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;

namespace StockBite.Application.Tenants.Commands;

public record UpdateTenantStatusCommand(Guid Id, bool IsActive) : IRequest;

public class UpdateTenantStatusCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateTenantStatusCommand>
{
    public async Task Handle(UpdateTenantStatusCommand request, CancellationToken ct)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Tenant), request.Id);
        tenant.IsActive = request.IsActive;
        await db.SaveChangesAsync(ct);
    }
}
