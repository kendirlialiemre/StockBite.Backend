namespace StockBite.Domain.Entities;

public class MemberSubscription : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;

    /// <summary>Pakette toplam saat (örn. 30)</summary>
    public decimal TotalHours { get; set; }

    /// <summary>Kalan saat</summary>
    public decimal RemainingHours { get; set; }

    /// <summary>Ödenen tutar</summary>
    public decimal Price { get; set; }

    public string? Note { get; set; }
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SubscriptionSession> Sessions { get; set; } = new List<SubscriptionSession>();
}
