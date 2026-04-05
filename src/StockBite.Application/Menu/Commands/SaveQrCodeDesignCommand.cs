using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Entities;

namespace StockBite.Application.Menu.Commands;

public record SaveQrCodeDesignCommand(
    Guid QrCodeId,
    int? QrMenuTemplate,
    string? PrimaryColor,
    string? BgColor,
    string? TextColor,
    string? FontFamily
) : IRequest;

public class SaveQrCodeDesignCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<SaveQrCodeDesignCommand>
{
    public async Task Handle(SaveQrCodeDesignCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException();

        var qr = await db.MenuQrCodes
            .FirstOrDefaultAsync(q => q.Id == request.QrCodeId && q.TenantId == tenantId, ct)
            ?? throw new NotFoundException(nameof(MenuQrCode), request.QrCodeId);

        if (request.QrMenuTemplate.HasValue) qr.QrMenuTemplate = request.QrMenuTemplate;
        if (!string.IsNullOrWhiteSpace(request.PrimaryColor)) qr.PrimaryColor = request.PrimaryColor;
        if (!string.IsNullOrWhiteSpace(request.BgColor)) qr.BgColor = request.BgColor;
        if (!string.IsNullOrWhiteSpace(request.TextColor)) qr.TextColor = request.TextColor;
        if (!string.IsNullOrWhiteSpace(request.FontFamily)) qr.FontFamily = request.FontFamily;

        await db.SaveChangesAsync(ct);
    }
}
