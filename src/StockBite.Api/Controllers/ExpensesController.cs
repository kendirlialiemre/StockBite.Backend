using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockBite.Application.Expenses.Commands;
using StockBite.Application.Expenses.Queries;
using StockBite.Domain.Enums;

namespace StockBite.Api.Controllers;

public record CreateExpenseRequest(decimal Amount, ExpenseCategory Category, string Description, DateOnly Date);

[ApiController]
[Route("api/expenses")]
[Authorize]
public class ExpensesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetExpenses([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct) =>
        Ok(await mediator.Send(new GetExpensesQuery(from, to), ct));

    [HttpPost]
    public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateExpenseCommand(req.Amount, req.Category, req.Description, req.Date), ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteExpense(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteExpenseCommand(id), ct);
        return NoContent();
    }
}
