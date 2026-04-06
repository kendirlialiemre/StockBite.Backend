using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;

namespace StockBite.Application.Menu.Commands;

public record DeleteMenuItemCommand(Guid ItemId) : IRequest;

public class DeleteMenuItemCommandHandler(IApplicationDbContext db, IStorageService storage)
    : IRequestHandler<DeleteMenuItemCommand>
{
    public async Task Handle(DeleteMenuItemCommand request, CancellationToken ct)
    {
        var item = await db.MenuItems
            .FirstOrDefaultAsync(i => i.Id == request.ItemId, ct)
            ?? throw new NotFoundException("MenuItem", request.ItemId);

        if (!string.IsNullOrEmpty(item.ImageUrl))
            await storage.DeleteAsync(item.ImageUrl, ct);

        db.MenuItems.Remove(item);
        await db.SaveChangesAsync(ct);
    }
}
