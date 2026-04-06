using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.ProfitLoss.DTOs;
using StockBite.Domain.Enums;

namespace StockBite.Application.ProfitLoss.Queries;

public record GetReportRangeQuery(DateOnly From, DateOnly To) : IRequest<ReportRangeDto>;

public class GetReportRangeQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetReportRangeQuery, ReportRangeDto>
{
    public async Task<ReportRangeDto> Handle(GetReportRangeQuery request, CancellationToken ct)
    {
        var fromDt = request.From.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toDt = request.To.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var orders = await db.Orders
            .Include(o => o.Items)
            .Where(o => o.Status == OrderStatus.Closed && o.ClosedAt >= fromDt && o.ClosedAt <= toDt)
            .ToListAsync(ct);

        var summaries = await db.DailySummaries
            .Where(s => s.Date >= request.From && s.Date <= request.To)
            .ToListAsync(ct);

        var expenses = await db.Expenses
            .Where(e => e.Date >= request.From && e.Date <= request.To)
            .ToListAsync(ct);

        var orderDates = orders.Select(o => DateOnly.FromDateTime(o.ClosedAt!.Value)).Distinct();
        var summaryDates = summaries.Select(s => s.Date).Distinct();
        var expenseDates = expenses.Select(e => e.Date).Distinct();
        var allDates = orderDates.Union(summaryDates).Union(expenseDates).OrderBy(d => d).ToList();

        var dailyGroups = allDates.Select(date =>
        {
            var dayOrders = orders.Where(o => DateOnly.FromDateTime(o.ClosedAt!.Value) == date).ToList();
            var daySummary = summaries.FirstOrDefault(s => s.Date == date);
            var dayExpenses = expenses.Where(e => e.Date == date).Sum(e => e.Amount);

            var revenue = dayOrders.Sum(o => o.TotalAmount);
            var cashRevenue = dayOrders.Where(o => o.PaymentMethod == PaymentMethod.Cash).Sum(o => o.TotalAmount);
            var cardRevenue = dayOrders.Where(o => o.PaymentMethod == PaymentMethod.Card).Sum(o => o.TotalAmount);
            var cost = dayOrders.Sum(o => o.Items.Sum(i => (i.UnitCost ?? 0) * i.Quantity));
            var stockPurchaseCost = daySummary?.StockPurchaseCost ?? 0;
            var grossProfit = revenue - cost - stockPurchaseCost - dayExpenses;

            return new DailySummaryDto(date, revenue, cashRevenue, cardRevenue, cost, stockPurchaseCost, dayExpenses, grossProfit, dayOrders.Count);
        }).ToList();

        return new ReportRangeDto(
            request.From, request.To,
            dailyGroups.Sum(d => d.TotalRevenue),
            dailyGroups.Sum(d => d.CashRevenue),
            dailyGroups.Sum(d => d.CardRevenue),
            dailyGroups.Sum(d => d.TotalCost),
            dailyGroups.Sum(d => d.StockPurchaseCost),
            dailyGroups.Sum(d => d.OtherExpenses),
            dailyGroups.Sum(d => d.GrossProfit),
            dailyGroups.Sum(d => d.OrderCount),
            dailyGroups);
    }
}
