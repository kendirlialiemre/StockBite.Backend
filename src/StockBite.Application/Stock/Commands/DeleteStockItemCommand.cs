using MediatR;
using StockBite.Application.Common.Interfaces;

namespace StockBite.Application.Stock.Commands;

public record DeleteStockItemCommand(Guid Id) : IRequest;

public class DeleteStockItemCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<DeleteStockItemCommand>
{
    public async Task Handle(DeleteStockItemCommand request, CancellationToken ct)
    {
        var item = await db.StockItems.FindAsync([request.Id], ct)
            ?? throw new Exception("Stok kalemi bulunamadı.");

        if (item.TenantId != currentUser.TenantId)
            throw new Exception("Yetkisiz erişim.");

        db.StockItems.Remove(item);
        await db.SaveChangesAsync(ct);
    }
}
