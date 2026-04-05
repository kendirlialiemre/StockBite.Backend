using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Menu.DTOs;

namespace StockBite.Application.Menu.Queries;

public record GetMenuItemsQuery(Guid? CategoryId = null) : IRequest<List<MenuItemDto>>;

public class GetMenuItemsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetMenuItemsQuery, List<MenuItemDto>>
{
    public async Task<List<MenuItemDto>> Handle(GetMenuItemsQuery request, CancellationToken ct)
    {
        var query = db.MenuItems
            .Include(i => i.Category)
            .Include(i => i.Ingredients).ThenInclude(ing => ing.StockItem)
            .AsQueryable();

        if (request.CategoryId.HasValue)
            query = query.Where(i => i.CategoryId == request.CategoryId.Value);

        var items = await query.OrderBy(i => i.Name).ToListAsync(ct);

        return items.Select(i => MapToDto(i)).ToList();
    }

    internal static MenuItemDto MapToDto(Domain.Entities.MenuItem i)
    {
        var ingredients = i.Ingredients.Select(ing => new MenuItemIngredientDto(
            ing.Id, ing.StockItemId, ing.StockItem.Name, ing.StockItem.Unit,
            ing.Quantity, ing.StockItem.UnitCost)).ToList();

        decimal? calculatedCost = ingredients.Any(ing => ing.UnitCost.HasValue)
            ? ingredients.Sum(ing => ing.Quantity * (ing.UnitCost ?? 0))
            : null;

        decimal? estimatedProfit = calculatedCost.HasValue ? i.Price - calculatedCost.Value : null;

        return new MenuItemDto(i.Id, i.CategoryId, i.Category?.Name,
            i.Name, i.Description, i.Price, i.CostPrice, i.ImageUrl, i.IsAvailable,
            ingredients, calculatedCost, estimatedProfit);
    }
}
