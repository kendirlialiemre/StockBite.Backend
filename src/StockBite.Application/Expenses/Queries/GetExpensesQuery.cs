using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Expenses.DTOs;

namespace StockBite.Application.Expenses.Queries;

public record GetExpensesQuery(DateOnly? From, DateOnly? To) : IRequest<List<ExpenseDto>>;

public class GetExpensesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetExpensesQuery, List<ExpenseDto>>
{
    public async Task<List<ExpenseDto>> Handle(GetExpensesQuery request, CancellationToken ct)
    {
        var query = db.Expenses.AsQueryable();

        if (request.From.HasValue)
            query = query.Where(e => e.Date >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(e => e.Date <= request.To.Value);

        var expenses = await query.OrderByDescending(e => e.Date).ThenByDescending(e => e.CreatedAt).ToListAsync(ct);

        return expenses.Select(Commands.CreateExpenseCommandHandler.ToDto).ToList();
    }
}
