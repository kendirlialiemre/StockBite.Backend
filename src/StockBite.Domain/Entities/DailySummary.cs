namespace StockBite.Domain.Entities;

public class DailySummary
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public DateOnly Date { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal CashRevenue { get; set; }
    public decimal CardRevenue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal StockPurchaseCost { get; set; }
    public decimal GrossProfit { get; set; }
    public int OrderCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
