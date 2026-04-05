using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Entities;

namespace StockBite.Application.Menu.Queries;

public record QrCodeDesignDto(
    int QrMenuTemplate,
    string PrimaryColor,
    string BgColor,
    string TextColor,
    string FontFamily,
    string? LogoUrl
);

public record GetQrCodeDesignQuery(Guid QrCodeId) : IRequest<QrCodeDesignDto>;

public class GetQrCodeDesignQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GetQrCodeDesignQuery, QrCodeDesignDto>
{
    public async Task<QrCodeDesignDto> Handle(GetQrCodeDesignQuery request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException();

        var tenant = await db.Tenants
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId, ct)
            ?? throw new NotFoundException(nameof(Tenant), tenantId);

        var qr = await db.MenuQrCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == request.QrCodeId, ct)
            ?? throw new NotFoundException(nameof(MenuQrCode), request.QrCodeId);

        // Null → inherit from tenant default
        return new QrCodeDesignDto(
            qr.QrMenuTemplate ?? tenant.QrMenuTemplate,
            qr.PrimaryColor ?? tenant.PrimaryColor,
            qr.BgColor ?? tenant.BgColor,
            qr.TextColor ?? tenant.TextColor,
            qr.FontFamily ?? tenant.FontFamily,
            qr.LogoUrl ?? tenant.LogoUrl
        );
    }
}
