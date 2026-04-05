namespace StockBite.Application.ProfitLoss.DTOs;

public record DailySummaryDto(DateOnly Date, decimal TotalRevenue, decimal TotalCost, decimal GrossProfit, int OrderCount);
public record ReportRangeDto(DateOnly From, DateOnly To, decimal TotalRevenue, decimal TotalCost, decimal GrossProfit, int TotalOrders, IReadOnlyList<DailySummaryDto> DailySummaries);
