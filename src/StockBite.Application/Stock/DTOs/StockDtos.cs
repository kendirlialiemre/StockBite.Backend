using StockBite.Domain.Enums;

namespace StockBite.Application.Stock.DTOs;

public record StockCategoryDto(Guid Id, string Name);
public record StockItemDto(Guid Id, Guid? CategoryId, string? CategoryName, string Name, string Unit, decimal Quantity, decimal? LowStockThreshold, bool IsLowStock, decimal? UnitCost);
public record StockMovementDto(Guid Id, Guid StockItemId, string StockItemName, StockMovementType Type, decimal Quantity, decimal? UnitCost, string? Note, DateTime CreatedAt);
public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
