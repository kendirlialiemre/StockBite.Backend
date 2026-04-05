using StockBite.Domain.Enums;
namespace StockBite.Domain.Entities;
public class PackageModule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PackageId { get; set; }
    public Package Package { get; set; } = null!;
    public ModuleType ModuleType { get; set; }
}
