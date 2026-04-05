namespace StockBite.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Note { get; set; }
}
