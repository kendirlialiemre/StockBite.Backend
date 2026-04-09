using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Orders.DTOs;
using StockBite.Domain.Entities;

namespace StockBite.Application.Orders.Commands;

public record AddOrderItemCommand(Guid OrderId, Guid MenuItemId, int Quantity, string? Note) : IRequest<OrderDto>;

public class AddOrderItemCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AddOrderItemCommand, OrderDto>
{
    public async Task<OrderDto> Handle(AddOrderItemCommand request, CancellationToken ct)
    {
        var order = await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .Include(o => o.Table)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        var menuItem = await db.MenuItems
            .FirstOrDefaultAsync(m => m.Id == request.MenuItemId, ct)
            ?? throw new NotFoundException(nameof(MenuItem), request.MenuItemId);

        var item = new OrderItem
        {
            TenantId = order.TenantId,
            OrderId = order.Id,
            MenuItemId = menuItem.Id,
            MenuItem = menuItem,
            Quantity = request.Quantity,
            UnitPrice = menuItem.Price,
            UnitCost = menuItem.CostPrice,
            Note = request.Note
        };
        db.OrderItems.Add(item);
        order.TotalAmount = order.Items
            .Where(i => !ReferenceEquals(i, item))
            .Sum(i => i.UnitPrice * i.Quantity) + (item.UnitPrice * item.Quantity);
        await db.SaveChangesAsync(ct);

        return new OrderDto(order.Id, order.TableId, order.Table?.Name, order.Status,
            order.OpenedAt, order.ClosedAt, order.TotalAmount, order.Note, order.PaymentMethod, order.CashAmount, order.CardAmount,
            order.Items.Select(i => new OrderItemDto(
                i.Id, i.MenuItemId, i.MenuItem.Name, i.Quantity, i.UnitPrice, i.Note)).ToList());
    }
}

