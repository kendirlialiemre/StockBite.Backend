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

        var dailyGroups = orders
            .GroupBy(o => DateOnly.FromDateTime(o.ClosedAt!.Value))
            .Select(g =>
            {
                var revenue = g.Sum(o => o.TotalAmount);
                var cost = g.Sum(o => o.Items.Sum(i => (i.UnitCost ?? 0) * i.Quantity));
                return new DailySummaryDto(g.Key, revenue, cost, revenue - cost, g.Count());
            })
            .OrderBy(d => d.Date)
            .ToList();

        return new ReportRangeDto(
            request.From, request.To,
            dailyGroups.Sum(d => d.TotalRevenue),
            dailyGroups.Sum(d => d.TotalCost),
            dailyGroups.Sum(d => d.GrossProfit),
            dailyGroups.Sum(d => d.OrderCount),
            dailyGroups);
    }
}
