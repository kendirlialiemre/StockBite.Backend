using MediatR;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Menu.Queries;
using StockBite.Domain.Entities;

namespace StockBite.Application.Menu.Commands;

public record CreateMenuQrCodeCommand(Guid Id, Guid TenantId, string Label, string FilePath, string PublicUrl) : IRequest<MenuQrCodeDto>;

public class CreateMenuQrCodeCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateMenuQrCodeCommand, MenuQrCodeDto>
{
    public async Task<MenuQrCodeDto> Handle(CreateMenuQrCodeCommand request, CancellationToken ct)
    {
        var qr = new MenuQrCode
        {
            Id = request.Id,
            TenantId = request.TenantId,
            Label = request.Label,
            FilePath = request.FilePath,
            PublicUrl = request.PublicUrl,
        };

        db.MenuQrCodes.Add(qr);
        await db.SaveChangesAsync(ct);

        return new MenuQrCodeDto(qr.Id, qr.Label, qr.FilePath, qr.PublicUrl, qr.CreatedAt);
    }
}
