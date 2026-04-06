namespace StockBite.Application.ProfitLoss.DTOs;

public record DailySummaryDto(DateOnly Date, decimal TotalRevenue, decimal CashRevenue, decimal CardRevenue, decimal TotalCost, decimal StockPurchaseCost, decimal OtherExpenses, decimal GrossProfit, int OrderCount);
public record ReportRangeDto(DateOnly From, DateOnly To, decimal TotalRevenue, decimal CashRevenue, decimal CardRevenue, decimal TotalCost, decimal StockPurchaseCost, decimal OtherExpenses, decimal GrossProfit, int TotalOrders, IReadOnlyList<DailySummaryDto> DailySummaries);
