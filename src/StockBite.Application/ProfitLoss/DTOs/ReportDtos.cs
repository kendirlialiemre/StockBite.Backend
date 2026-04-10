namespace StockBite.Application.ProfitLoss.DTOs;

public record DailySummaryDto(DateOnly Date, decimal TotalRevenue, decimal CashRevenue, decimal CardRevenue, decimal SubscriptionRevenue, decimal EventRevenue, decimal TotalCost, decimal StockPurchaseCost, decimal OtherExpenses, decimal GrossProfit, int OrderCount, int TableCount);
public record TopProductDto(string Name, int TotalQuantity, decimal TotalRevenue);
public record ReportRangeDto(DateOnly From, DateOnly To, decimal TotalRevenue, decimal CashRevenue, decimal CardRevenue, decimal SubscriptionRevenue, decimal EventRevenue, decimal TotalCost, decimal StockPurchaseCost, decimal OtherExpenses, decimal GrossProfit, int TotalOrders, IReadOnlyList<DailySummaryDto> DailySummaries, IReadOnlyList<TopProductDto> TopProducts);
