namespace StockBite.Domain.Entities;
public class Package : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int? DurationDays { get; set; }   // null = sınırsız
    public bool IsActive { get; set; } = true;
    public ICollection<PackageModule> Modules { get; set; } = new List<PackageModule>();
}
