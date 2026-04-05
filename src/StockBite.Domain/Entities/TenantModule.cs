using StockBite.Domain.Enums;

namespace StockBite.Domain.Entities;

public class TenantModule : BaseEntity
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public ModuleType ModuleType { get; set; }
    public bool IsActive { get; set; } = true;
    public bool GrantedByAdmin { get; set; } = false;
    public DateTime StartsAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
}
