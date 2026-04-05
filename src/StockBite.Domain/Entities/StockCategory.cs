namespace StockBite.Domain.Entities;

public class StockCategory : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<StockItem> Items { get; set; } = new List<StockItem>();
}
