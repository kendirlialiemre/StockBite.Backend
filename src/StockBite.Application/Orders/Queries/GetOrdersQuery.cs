using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Orders.DTOs;
using StockBite.Domain.Enums;

namespace StockBite.Application.Orders.Queries;

public record GetOrdersQuery(OrderStatus? Status = null) : IRequest<List<OrderDto>>;

public class GetOrdersQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetOrdersQuery, List<OrderDto>>
{
    public async Task<List<OrderDto>> Handle(GetOrdersQuery request, CancellationToken ct)
    {
        var query = db.Orders
            .Include(o => o.Table)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(o => o.Status == request.Status.Value);

        return await query.OrderByDescending(o => o.OpenedAt)
            .Select(o => new OrderDto(
                o.Id, o.TableId, o.Table != null ? o.Table.Name : null,
                o.Status, o.OpenedAt, o.ClosedAt, o.TotalAmount, o.Note,
                o.Items.Select(i => new OrderItemDto(
                    i.Id, i.MenuItemId, i.MenuItem.Name, i.Quantity, i.UnitPrice, i.Note)).ToList()))
            .ToListAsync(ct);
    }
}
