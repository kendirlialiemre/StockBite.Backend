namespace StockBite.Application.Menu.DTOs;

public record MenuCategoryDto(Guid Id, string Name, int DisplayOrder, bool IsActive);

public record MenuItemIngredientDto(Guid Id, Guid StockItemId, string StockItemName, string Unit, decimal Quantity, decimal? UnitCost);

public record MenuItemDto(
    Guid Id,
    Guid? CategoryId,
    string? CategoryName,
    string Name,
    string? Description,
    decimal Price,
    decimal? CostPrice,
    string? ImageUrl,
    bool IsAvailable,
    List<MenuItemIngredientDto> Ingredients,
    decimal? CalculatedCost,
    decimal? EstimatedProfit);
