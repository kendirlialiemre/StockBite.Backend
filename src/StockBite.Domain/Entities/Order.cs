using StockBite.Domain.Enums;

namespace StockBite.Domain.Entities;

public class Order : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid? TableId { get; set; }
    public Table? Table { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Open;
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public Guid CreatedBy { get; set; }
    public string? Note { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
