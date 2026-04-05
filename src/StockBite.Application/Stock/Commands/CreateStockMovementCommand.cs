using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Stock.DTOs;
using StockBite.Domain.Entities;
using StockBite.Domain.Enums;

namespace StockBite.Application.Stock.Commands;

public record CreateStockMovementCommand(Guid StockItemId, StockMovementType Type, decimal Quantity, decimal? UnitCost, string? Note, decimal? LowStockThreshold = null) : IRequest<StockMovementDto>;

public class CreateStockMovementValidator : AbstractValidator<CreateStockMovementCommand>
{
    public CreateStockMovementValidator() { RuleFor(x => x.Quantity).GreaterThan(0); }
}

public class CreateStockMovementCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<CreateStockMovementCommand, StockMovementDto>
{
    public async Task<StockMovementDto> Handle(CreateStockMovementCommand request, CancellationToken ct)
    {
        var stockItem = await db.StockItems.FirstOrDefaultAsync(s => s.Id == request.StockItemId, ct)
            ?? throw new NotFoundException(nameof(StockItem), request.StockItemId);

        var movement = new StockMovement
        {
            TenantId = currentUser.TenantId!.Value,
            StockItemId = request.StockItemId,
            StockItem = stockItem,
            Type = request.Type,
            Quantity = request.Quantity,
            UnitCost = request.UnitCost,
            Note = request.Note,
            CreatedBy = currentUser.UserId!.Value
        };
        db.StockMovements.Add(movement);

        stockItem.Quantity += request.Type switch
        {
            StockMovementType.StockIn => request.Quantity,
            StockMovementType.StockOut => -request.Quantity,
            StockMovementType.Adjustment => request.Quantity - stockItem.Quantity,
            _ => 0
        };

        // Update unit cost on stock-in so ingredients can compute cost
        if (request.Type == StockMovementType.StockIn && request.UnitCost.HasValue)
            stockItem.UnitCost = request.UnitCost.Value;

        // Update low stock threshold if provided
        if (request.LowStockThreshold.HasValue)
            stockItem.LowStockThreshold = request.LowStockThreshold.Value;

        await db.SaveChangesAsync(ct);
        return new StockMovementDto(movement.Id, movement.StockItemId, stockItem.Name,
            movement.Type, movement.Quantity, movement.UnitCost, movement.Note, movement.CreatedAt);
    }
}
