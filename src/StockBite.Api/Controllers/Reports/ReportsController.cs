using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockBite.Api.Filters;
using StockBite.Application.ProfitLoss.Queries;
using StockBite.Domain.Constants;
using StockBite.Domain.Enums;

namespace StockBite.Api.Controllers.Reports;

[ApiController]
[Route("api/reports")]
[Authorize]
[RequireModule(ModuleType.ProfitLoss)]
public class ReportsController(IMediator mediator) : ControllerBase
{
    [HttpGet("range")]
    [RequirePermission(Permissions.ProfitLoss.View)]
    public async Task<IActionResult> GetRange([FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct) =>
        Ok(await mediator.Send(new GetReportRangeQuery(from, to), ct));

    [HttpGet("daily")]
    [RequirePermission(Permissions.ProfitLoss.View)]
    public async Task<IActionResult> GetDaily([FromQuery] DateOnly? date, CancellationToken ct)
    {
        var d = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await mediator.Send(new GetReportRangeQuery(d, d), ct);
        return Ok(result.DailySummaries.FirstOrDefault());
    }
}
