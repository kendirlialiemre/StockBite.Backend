namespace StockBite.Domain.Entities;

public class StockItem : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid? CategoryId { get; set; }
    public StockCategory? Category { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 0;
    public decimal? LowStockThreshold { get; set; }
    public decimal? UnitCost { get; set; }
    public ICollection<StockMovement> Movements { get; set; } = new List<StockMovement>();
}
