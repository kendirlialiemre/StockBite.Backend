using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Entities;

namespace StockBite.Application.Menu.Queries;

public record MenuSettingsDto(int QrMenuTemplate, string? LogoUrl, string PrimaryColor, string BgColor, string TextColor, string FontFamily);

public record GetMenuSettingsQuery : IRequest<MenuSettingsDto>;

public class GetMenuSettingsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GetMenuSettingsQuery, MenuSettingsDto>
{
    public async Task<MenuSettingsDto> Handle(GetMenuSettingsQuery request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException();

        var tenant = await db.Tenants
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId, ct)
            ?? throw new NotFoundException(nameof(Tenant), tenantId);

        return new MenuSettingsDto(tenant.QrMenuTemplate, tenant.LogoUrl, tenant.PrimaryColor, tenant.BgColor, tenant.TextColor, tenant.FontFamily);
    }
}
