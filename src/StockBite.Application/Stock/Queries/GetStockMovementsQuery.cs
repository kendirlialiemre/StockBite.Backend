using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Stock.DTOs;

namespace StockBite.Application.Stock.Queries;

public record GetStockMovementsQuery(Guid? StockItemId = null, int Page = 1, int PageSize = 10) : IRequest<PagedResult<StockMovementDto>>;

public class GetStockMovementsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetStockMovementsQuery, PagedResult<StockMovementDto>>
{
    public async Task<PagedResult<StockMovementDto>> Handle(GetStockMovementsQuery request, CancellationToken ct)
    {
        var query = db.StockMovements.Include(m => m.StockItem).AsQueryable();

        if (request.StockItemId.HasValue)
            query = query.Where(m => m.StockItemId == request.StockItemId.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new StockMovementDto(
                m.Id, m.StockItemId, m.StockItem.Name,
                m.Type, m.Quantity, m.UnitCost, m.Note, m.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<StockMovementDto>(items, totalCount, request.Page, request.PageSize);
    }
}
