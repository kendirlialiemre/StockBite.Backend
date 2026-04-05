using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Entities;

namespace StockBite.Application.Menu.Commands;

/// <summary>Returns the FilePath so the controller can delete the file from disk.</summary>
public record DeleteMenuQrCodeCommand(Guid Id) : IRequest<string>;

public class DeleteMenuQrCodeCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteMenuQrCodeCommand, string>
{
    public async Task<string> Handle(DeleteMenuQrCodeCommand request, CancellationToken ct)
    {
        var qr = await db.MenuQrCodes.FirstOrDefaultAsync(q => q.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(MenuQrCode), request.Id);

        var filePath = qr.FilePath;
        db.MenuQrCodes.Remove(qr);
        await db.SaveChangesAsync(ct);

        return filePath;
    }
}
