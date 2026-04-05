using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Menu.DTOs;
using StockBite.Application.Menu.Queries;
using StockBite.Domain.Entities;

namespace StockBite.Application.Menu.Commands;

public record UpdateMenuItemCommand(
    Guid Id,
    string? Name,
    string? Description,
    decimal? Price,
    decimal? CostPrice,
    Guid? CategoryId,
    bool? IsAvailable,
    List<IngredientRequest>? Ingredients = null) : IRequest<MenuItemDto>;

public class UpdateMenuItemCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateMenuItemCommand, MenuItemDto>
{
    public async Task<MenuItemDto> Handle(UpdateMenuItemCommand request, CancellationToken ct)
    {
        var item = await db.MenuItems
            .FirstOrDefaultAsync(i => i.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(MenuItem), request.Id);

        if (request.Name != null) item.Name = request.Name;
        if (request.Description != null) item.Description = request.Description;
        if (request.Price.HasValue) item.Price = request.Price.Value;
        if (request.CostPrice.HasValue) item.CostPrice = request.CostPrice.Value;
        if (request.CategoryId.HasValue) item.CategoryId = request.CategoryId.Value;
        if (request.IsAvailable.HasValue) item.IsAvailable = request.IsAvailable.Value;

        if (request.Ingredients != null)
        {
            var existing = await db.MenuItemIngredients
                .Where(i => i.MenuItemId == request.Id).ToListAsync(ct);
            db.MenuItemIngredients.RemoveRange(existing);

            foreach (var ing in request.Ingredients.Where(i => i.Quantity > 0))
            {
                db.MenuItemIngredients.Add(new MenuItemIngredient
                {
                    MenuItemId = item.Id,
                    StockItemId = ing.StockItemId,
                    Quantity = ing.Quantity,
                });
            }
        }

        await db.SaveChangesAsync(ct);

        var loaded = await db.MenuItems
            .Include(i => i.Category)
            .Include(i => i.Ingredients).ThenInclude(ing => ing.StockItem)
            .FirstAsync(i => i.Id == item.Id, ct);

        return GetMenuItemsQueryHandler.MapToDto(loaded);
    }
}
