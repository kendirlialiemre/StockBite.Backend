namespace StockBite.Domain.Entities;

public class MenuItemIngredient : BaseEntity
{
    public Guid MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
    public Guid StockItemId { get; set; }
    public StockItem StockItem { get; set; } = null!;
    public decimal Quantity { get; set; }
}
