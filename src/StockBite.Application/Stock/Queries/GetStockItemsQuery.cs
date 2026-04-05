using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Stock.DTOs;

namespace StockBite.Application.Stock.Queries;

public record GetStockItemsQuery : IRequest<List<StockItemDto>>;

public class GetStockItemsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetStockItemsQuery, List<StockItemDto>>
{
    public async Task<List<StockItemDto>> Handle(GetStockItemsQuery request, CancellationToken ct)
    {
        return await db.StockItems.Include(s => s.Category).OrderBy(s => s.Name)
            .Select(s => new StockItemDto(
                s.Id, s.CategoryId,
                s.Category != null ? s.Category.Name : null,
                s.Name, s.Unit, s.Quantity, s.LowStockThreshold,
                s.LowStockThreshold.HasValue && s.Quantity <= s.LowStockThreshold.Value,
                s.UnitCost))
            .ToListAsync(ct);
    }
}
