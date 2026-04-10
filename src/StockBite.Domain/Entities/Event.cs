using StockBite.Domain.Enums;

namespace StockBite.Domain.Entities;

public class Event : BaseEntity
{
    public Guid TenantId { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public int? Age { get; set; }
    public DateOnly EventDate { get; set; }
    public int AdultCount { get; set; }
    public int ChildCount { get; set; }
    public string EventType { get; set; } = "Doğum Günü"; // Doğum Günü, Sünnet, Nikah, Diğer
    public string? Package { get; set; }  // Yemekli, Pastasız, Tam Paket, vb.
    public decimal ChargedAmount { get; set; }
    public decimal Cost { get; set; }
    public string? Notes { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Planned;

    // Ödeme
    public PaymentMethod? PaymentMethod { get; set; }
    public decimal CashAmount { get; set; }
    public decimal CardAmount { get; set; }
    public DateTime? PaidAt { get; set; }
}
