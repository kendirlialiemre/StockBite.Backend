using StockBite.Domain.Enums;

namespace StockBite.Domain.Entities;

public class StockMovement : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid StockItemId { get; set; }
    public StockItem StockItem { get; set; } = null!;
    public StockMovementType Type { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Note { get; set; }
    public Guid CreatedBy { get; set; }
}
