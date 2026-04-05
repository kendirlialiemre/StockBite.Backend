using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Entities;

namespace StockBite.Application.Public;

public record SaveQrTemplateCommand(int Template) : IRequest;

public class SaveQrTemplateCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<SaveQrTemplateCommand>
{
    public async Task Handle(SaveQrTemplateCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException();

        var tenant = await db.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId, ct)
            ?? throw new NotFoundException(nameof(Tenant), tenantId);

        tenant.QrMenuTemplate = request.Template;
        await db.SaveChangesAsync(ct);
    }
}
