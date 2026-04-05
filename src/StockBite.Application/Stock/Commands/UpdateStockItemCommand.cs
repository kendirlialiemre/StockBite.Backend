using MediatR;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Stock.DTOs;

namespace StockBite.Application.Stock.Commands;

public record UpdateStockItemCommand(Guid Id, string? Name, string? Unit, decimal? LowStockThreshold) : IRequest<StockItemDto>;

public class UpdateStockItemCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<UpdateStockItemCommand, StockItemDto>
{
    public async Task<StockItemDto> Handle(UpdateStockItemCommand request, CancellationToken ct)
    {
        var item = await db.StockItems.FindAsync([request.Id], ct)
            ?? throw new Exception("Stok kalemi bulunamadı.");

        if (item.TenantId != currentUser.TenantId)
            throw new Exception("Yetkisiz erişim.");

        if (request.Name is not null) item.Name = request.Name;
        if (request.Unit is not null) item.Unit = request.Unit;
        if (request.LowStockThreshold.HasValue) item.LowStockThreshold = request.LowStockThreshold;

        await db.SaveChangesAsync(ct);

        return new StockItemDto(item.Id, item.CategoryId, null,
            item.Name, item.Unit, item.Quantity, item.LowStockThreshold,
            item.LowStockThreshold.HasValue && item.Quantity <= item.LowStockThreshold,
            item.UnitCost);
    }
}
