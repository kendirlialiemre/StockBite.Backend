using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Orders.DTOs;
using StockBite.Domain.Enums;

namespace StockBite.Application.Orders.Queries;

public record GetTablesQuery : IRequest<List<TableWithOrderDto>>;

public class GetTablesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetTablesQuery, List<TableWithOrderDto>>
{
    public async Task<List<TableWithOrderDto>> Handle(GetTablesQuery request, CancellationToken ct)
    {
        var tables = await db.Tables
            .Where(t => t.IsActive)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(ct);

        if (tables.Count == 0) return [];

        var tableIds = tables.Select(t => t.Id).ToList();

        var openOrders = await db.Orders
            .Include(o => o.Items)
            .Where(o => o.Status == OrderStatus.Open && o.TableId.HasValue && tableIds.Contains(o.TableId.Value))
            .ToListAsync(ct);

        return tables.Select(t =>
        {
            var order = openOrders.FirstOrDefault(o => o.TableId == t.Id);
            return new TableWithOrderDto(
                t.Id,
                t.Name,
                order?.Id,
                order?.TotalAmount ?? 0,
                order?.Items.Count ?? 0,
                order?.OpenedAt ?? t.CreatedAt
            );
        }).ToList();
    }
}
