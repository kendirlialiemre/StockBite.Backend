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
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .Where(o => o.Status == OrderStatus.Closed && o.ClosedAt >= fromDt && o.ClosedAt <= toDt)
            .ToListAsync(ct);

        var summaries = await db.DailySummaries
            .Where(s => s.Date >= request.From && s.Date <= request.To)
            .ToListAsync(ct);

        var expenses = await db.Expenses
            .Where(e => e.Date >= request.From && e.Date <= request.To)
            .ToListAsync(ct);

        var subscriptions = await db.MemberSubscriptions
            .Where(s => s.PurchasedAt >= fromDt && s.PurchasedAt <= toDt)
            .ToListAsync(ct);

        var orderDates = orders.Select(o => DateOnly.FromDateTime(o.ClosedAt!.Value)).Distinct();
        var summaryDates = summaries.Select(s => s.Date).Distinct();
        var expenseDates = expenses.Select(e => e.Date).Distinct();
        var subDates = subscriptions.Select(s => DateOnly.FromDateTime(s.PurchasedAt)).Distinct();
        var allDates = orderDates.Union(summaryDates).Union(expenseDates).Union(subDates).OrderBy(d => d).ToList();

        var dailyGroups = allDates.Select(date =>
        {
            var dayOrders = orders.Where(o => DateOnly.FromDateTime(o.ClosedAt!.Value) == date).ToList();
            var daySummary = summaries.FirstOrDefault(s => s.Date == date);
            var dayExpenses = expenses.Where(e => e.Date == date).Sum(e => e.Amount);
            var daySubRevenue = subscriptions.Where(s => DateOnly.FromDateTime(s.PurchasedAt) == date).Sum(s => s.Price);

            var orderRevenue = dayOrders.Sum(o => o.TotalAmount);
            var cashRevenue = dayOrders.Where(o => o.PaymentMethod == PaymentMethod.Cash).Sum(o => o.TotalAmount);
            var cardRevenue = dayOrders.Where(o => o.PaymentMethod == PaymentMethod.Card).Sum(o => o.TotalAmount);
            var totalRevenue = orderRevenue + daySubRevenue;
            var cost = dayOrders.Sum(o => o.Items.Sum(i => (i.UnitCost ?? 0) * i.Quantity));
            var stockPurchaseCost = daySummary?.StockPurchaseCost ?? 0;
            var grossProfit = totalRevenue - cost - stockPurchaseCost - dayExpenses;
            var tableCount = dayOrders.Count(o => o.TableId.HasValue);

            return new DailySummaryDto(date, totalRevenue, cashRevenue, cardRevenue, daySubRevenue, cost, stockPurchaseCost, dayExpenses, grossProfit, dayOrders.Count, tableCount);
        }).ToList();

        var topProducts = orders
            .SelectMany(o => o.Items)
            .GroupBy(i => i.MenuItem.Name)
            .Select(g => new TopProductDto(g.Key, g.Sum(i => i.Quantity), g.Sum(i => i.UnitPrice * i.Quantity)))
            .OrderByDescending(p => p.TotalQuantity)
            .Take(5)
            .ToList();

        return new ReportRangeDto(
            request.From, request.To,
            dailyGroups.Sum(d => d.TotalRevenue),
            dailyGroups.Sum(d => d.CashRevenue),
            dailyGroups.Sum(d => d.CardRevenue),
            dailyGroups.Sum(d => d.SubscriptionRevenue),
            dailyGroups.Sum(d => d.TotalCost),
            dailyGroups.Sum(d => d.StockPurchaseCost),
            dailyGroups.Sum(d => d.OtherExpenses),
            dailyGroups.Sum(d => d.GrossProfit),
            dailyGroups.Sum(d => d.OrderCount),
            dailyGroups,
            topProducts);
    }
}
