namespace StockBite.Domain.Entities;

public class Member : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Note { get; set; }
    public ICollection<MemberSubscription> Subscriptions { get; set; } = new List<MemberSubscription>();
}
