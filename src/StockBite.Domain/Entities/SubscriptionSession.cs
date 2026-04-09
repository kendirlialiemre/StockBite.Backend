namespace StockBite.Domain.Entities;

public class SubscriptionSession : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid SubscriptionId { get; set; }
    public MemberSubscription Subscription { get; set; } = null!;

    /// <summary>Bu seansta düşülen saat</summary>
    public decimal Hours { get; set; }

    public string? Note { get; set; }
    public DateTime SessionAt { get; set; } = DateTime.UtcNow;
}
