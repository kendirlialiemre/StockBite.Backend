using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Menu.DTOs;
using StockBite.Application.Menu.Queries;
using StockBite.Domain.Entities;

namespace StockBite.Application.Menu.Commands;

public record IngredientRequest(Guid StockItemId, decimal Quantity);

public record CreateMenuItemCommand(
    string Name,
    decimal Price,
    Guid? CategoryId = null,
    string? Description = null,
    decimal? CostPrice = null,
    List<IngredientRequest>? Ingredients = null) : IRequest<MenuItemDto>;

public class CreateMenuItemValidator : AbstractValidator<CreateMenuItemCommand>
{
    public CreateMenuItemValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}

public class CreateMenuItemCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<CreateMenuItemCommand, MenuItemDto>
{
    public async Task<MenuItemDto> Handle(CreateMenuItemCommand request, CancellationToken ct)
    {
        var item = new MenuItem
        {
            TenantId = currentUser.TenantId!.Value,
            CategoryId = request.CategoryId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            CostPrice = request.CostPrice,
        };
        db.MenuItems.Add(item);
        await db.SaveChangesAsync(ct);

        if (request.Ingredients != null)
        {
            foreach (var ing in request.Ingredients.Where(i => i.Quantity > 0))
            {
                db.MenuItemIngredients.Add(new MenuItemIngredient
                {
                    MenuItemId = item.Id,
                    StockItemId = ing.StockItemId,
                    Quantity = ing.Quantity,
                });
            }
            await db.SaveChangesAsync(ct);
        }

        // Reload with full relations
        var loaded = await db.MenuItems
            .Include(i => i.Category)
            .Include(i => i.Ingredients).ThenInclude(ing => ing.StockItem)
            .FirstAsync(i => i.Id == item.Id, ct);

        return GetMenuItemsQueryHandler.MapToDto(loaded);
    }
}
