using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Menu.Queries;
using StockBite.Domain.Entities;

namespace StockBite.Application.Menu.Commands;

/// <summary>Updates the DB record with a new filePath and publicUrl. Returns old file path for cleanup.</summary>
public record RegenerateMenuQrCodeCommand(Guid Id, string NewFilePath, string NewPublicUrl) : IRequest<(MenuQrCodeDto Dto, string OldFilePath)>;

public class RegenerateMenuQrCodeCommandHandler(IApplicationDbContext db)
    : IRequestHandler<RegenerateMenuQrCodeCommand, (MenuQrCodeDto Dto, string OldFilePath)>
{
    public async Task<(MenuQrCodeDto Dto, string OldFilePath)> Handle(RegenerateMenuQrCodeCommand request, CancellationToken ct)
    {
        var qr = await db.MenuQrCodes.FirstOrDefaultAsync(q => q.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(MenuQrCode), request.Id);

        var oldFilePath = qr.FilePath;
        qr.FilePath = request.NewFilePath;
        qr.PublicUrl = request.NewPublicUrl;

        await db.SaveChangesAsync(ct);

        return (new MenuQrCodeDto(qr.Id, qr.Label, qr.FilePath, qr.PublicUrl, qr.CreatedAt), oldFilePath);
    }
}
