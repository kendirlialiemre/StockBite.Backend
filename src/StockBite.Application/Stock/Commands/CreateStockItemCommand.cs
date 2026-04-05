using FluentValidation;
using MediatR;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Stock.DTOs;
using StockBite.Domain.Entities;

namespace StockBite.Application.Stock.Commands;

public record CreateStockItemCommand(string Name, string Unit, decimal InitialQuantity = 0, decimal? UnitCost = null, decimal? LowStockThreshold = null, Guid? CategoryId = null) : IRequest<StockItemDto>;

public class CreateStockItemValidator : AbstractValidator<CreateStockItemCommand>
{
    public CreateStockItemValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(50);
        RuleFor(x => x.InitialQuantity).GreaterThanOrEqualTo(0);
    }
}

public class CreateStockItemCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<CreateStockItemCommand, StockItemDto>
{
    public async Task<StockItemDto> Handle(CreateStockItemCommand request, CancellationToken ct)
    {
        var item = new StockItem
        {
            TenantId = currentUser.TenantId!.Value,
            CategoryId = request.CategoryId,
            Name = request.Name,
            Unit = request.Unit,
            Quantity = request.InitialQuantity,
            UnitCost = request.UnitCost,
            LowStockThreshold = request.LowStockThreshold,
        };
        db.StockItems.Add(item);
        await db.SaveChangesAsync(ct);
        return new StockItemDto(item.Id, item.CategoryId, null,
            item.Name, item.Unit, item.Quantity, item.LowStockThreshold, false, item.UnitCost);
    }
}
