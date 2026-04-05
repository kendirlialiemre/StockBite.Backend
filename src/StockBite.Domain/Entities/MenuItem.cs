namespace StockBite.Domain.Entities;

public class MenuItem : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid? CategoryId { get; set; }
    public MenuCategory? Category { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? CostPrice { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; } = true;
    public ICollection<MenuItemIngredient> Ingredients { get; set; } = new List<MenuItemIngredient>();
}
