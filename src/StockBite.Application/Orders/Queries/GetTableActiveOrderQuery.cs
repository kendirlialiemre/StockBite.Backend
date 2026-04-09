using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Orders.DTOs;
using StockBite.Domain.Entities;
using StockBite.Domain.Enums;

namespace StockBite.Application.Orders.Queries;

public record GetTableActiveOrderQuery(Guid TableId) : IRequest<OrderDto>;

public class GetTableActiveOrderQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetTableActiveOrderQuery, OrderDto>
{
    public async Task<OrderDto> Handle(GetTableActiveOrderQuery request, CancellationToken ct)
    {
        var order = await db.Orders
            .Include(o => o.Table)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(o => o.TableId == request.TableId && o.Status == OrderStatus.Open, ct)
            ?? throw new NotFoundException("Açık sipariş bulunamadı.", request.TableId);

        return new OrderDto(order.Id, order.TableId, order.Table?.Name, order.Status,
            order.OpenedAt, order.ClosedAt, order.TotalAmount, order.Note, order.PaymentMethod, order.CashAmount, order.CardAmount,
            order.Items.Select(i => new OrderItemDto(
                i.Id, i.MenuItemId, i.MenuItem.Name, i.Quantity, i.UnitPrice, i.Note)).ToList());
    }
}

