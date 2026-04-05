using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Entities;

namespace StockBite.Application.Menu.Commands;

public record SaveMenuDesignCommand(
    int? QrMenuTemplate,
    string? PrimaryColor,
    string? BgColor,
    string? TextColor,
    string? FontFamily
) : IRequest;

public class SaveMenuDesignCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<SaveMenuDesignCommand>
{
    public async Task Handle(SaveMenuDesignCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException();

        var tenant = await db.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId, ct)
            ?? throw new NotFoundException(nameof(Tenant), tenantId);

        if (request.QrMenuTemplate.HasValue)
            tenant.QrMenuTemplate = request.QrMenuTemplate.Value;

        if (!string.IsNullOrWhiteSpace(request.PrimaryColor))
            tenant.PrimaryColor = request.PrimaryColor;

        if (!string.IsNullOrWhiteSpace(request.BgColor))
            tenant.BgColor = request.BgColor;

        if (!string.IsNullOrWhiteSpace(request.TextColor))
            tenant.TextColor = request.TextColor;

        if (!string.IsNullOrWhiteSpace(request.FontFamily))
            tenant.FontFamily = request.FontFamily;

        await db.SaveChangesAsync(ct);
    }
}
