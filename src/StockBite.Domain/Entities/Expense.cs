using StockBite.Domain.Enums;

namespace StockBite.Domain.Entities;

public class Expense : BaseEntity
{
    public Guid TenantId { get; set; }
    public decimal Amount { get; set; }
    public ExpenseCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
}
