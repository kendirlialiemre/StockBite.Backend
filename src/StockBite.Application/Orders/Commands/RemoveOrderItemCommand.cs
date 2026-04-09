using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Orders.DTOs;
using StockBite.Domain.Entities;

namespace StockBite.Application.Orders.Commands;

public record RemoveOrderItemCommand(Guid OrderId, Guid ItemId) : IRequest<OrderDto>;

public class RemoveOrderItemCommandHandler(IApplicationDbContext db)
    : IRequestHandler<RemoveOrderItemCommand, OrderDto>
{
    public async Task<OrderDto> Handle(RemoveOrderItemCommand request, CancellationToken ct)
    {
        var order = await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .Include(o => o.Table)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        var item = order.Items.FirstOrDefault(i => i.Id == request.ItemId)
            ?? throw new NotFoundException(nameof(OrderItem), request.ItemId);

        db.OrderItems.Remove(item);
        order.Items.Remove(item);
        order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);

        await db.SaveChangesAsync(ct);

        return new OrderDto(order.Id, order.TableId, order.Table?.Name, order.Status,
            order.OpenedAt, order.ClosedAt, order.TotalAmount, order.Note, order.PaymentMethod, order.CashAmount, order.CardAmount,
            order.Items.Select(i => new OrderItemDto(
                i.Id, i.MenuItemId, i.MenuItem.Name, i.Quantity, i.UnitPrice, i.Note)).ToList());
    }
}

