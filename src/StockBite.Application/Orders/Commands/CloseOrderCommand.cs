using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Entities;
using StockBite.Domain.Enums;

namespace StockBite.Application.Orders.Commands;

public record CloseOrderCommand(Guid OrderId) : IRequest;
public record CancelOrderCommand(Guid OrderId) : IRequest;

public class CloseOrderCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<CloseOrderCommand>
{
    public async Task Handle(CloseOrderCommand request, CancellationToken ct)
    {
        var order = await db.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.MenuItem)
                    .ThenInclude(m => m.Ingredients)
                        .ThenInclude(ing => ing.StockItem)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        if (order.Status != OrderStatus.Open)
            throw new InvalidOperationException("Sipariş zaten kapatılmış veya iptal edilmiş.");

        order.Status = OrderStatus.Closed;
        order.ClosedAt = DateTime.UtcNow;
        order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);

        // Deduct stock for each ingredient × order quantity
        decimal totalIngredientCost = 0;
        foreach (var orderItem in order.Items)
        {
            foreach (var ingredient in orderItem.MenuItem.Ingredients)
            {
                var deductQty = ingredient.Quantity * orderItem.Quantity;
                ingredient.StockItem.Quantity -= deductQty;

                if (ingredient.StockItem.UnitCost.HasValue)
                    totalIngredientCost += deductQty * ingredient.StockItem.UnitCost.Value;

                db.StockMovements.Add(new StockMovement
                {
                    TenantId = order.TenantId,
                    StockItemId = ingredient.StockItemId,
                    Type = StockMovementType.StockOut,
                    Quantity = deductQty,
                    UnitCost = ingredient.StockItem.UnitCost,
                    Note = $"Satış: {orderItem.MenuItem.Name}",
                    CreatedBy = currentUser.UserId!.Value
                });
            }
        }

        // Update daily summary — cost from ingredient unit costs
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var summary = await db.DailySummaries
            .FirstOrDefaultAsync(s => s.TenantId == order.TenantId && s.Date == today, ct);

        var totalCost = totalIngredientCost > 0
            ? totalIngredientCost
            : order.Items.Sum(i => (i.UnitCost ?? 0) * i.Quantity);

        if (summary == null)
        {
            db.DailySummaries.Add(new DailySummary
            {
                TenantId = order.TenantId,
                Date = today,
                TotalRevenue = order.TotalAmount,
                TotalCost = totalCost,
                GrossProfit = order.TotalAmount - totalCost,
                OrderCount = 1
            });
        }
        else
        {
            summary.TotalRevenue += order.TotalAmount;
            summary.TotalCost += totalCost;
            summary.GrossProfit += order.TotalAmount - totalCost;
            summary.OrderCount += 1;
        }

        if (order.TableId.HasValue)
        {
            var table = await db.Tables.FirstOrDefaultAsync(t => t.Id == order.TableId.Value, ct);
            if (table != null) table.IsActive = false;
        }

        await db.SaveChangesAsync(ct);
    }
}

public class CancelOrderCommandHandler(IApplicationDbContext db) : IRequestHandler<CancelOrderCommand>
{
    public async Task Handle(CancelOrderCommand request, CancellationToken ct)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        if (order.Status != OrderStatus.Open)
            throw new InvalidOperationException("Sipariş zaten kapatılmış veya iptal edilmiş.");

        order.Status = OrderStatus.Cancelled;
        order.ClosedAt = DateTime.UtcNow;

        if (order.TableId.HasValue)
        {
            var table = await db.Tables.FirstOrDefaultAsync(t => t.Id == order.TableId.Value, ct);
            if (table != null) table.IsActive = false;
        }

        await db.SaveChangesAsync(ct);
    }
}
