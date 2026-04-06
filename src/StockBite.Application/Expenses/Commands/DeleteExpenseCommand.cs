using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;

namespace StockBite.Application.Expenses.Commands;

public record DeleteExpenseCommand(Guid ExpenseId) : IRequest;

public class DeleteExpenseCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteExpenseCommand>
{
    public async Task Handle(DeleteExpenseCommand request, CancellationToken ct)
    {
        var expense = await db.Expenses.FirstOrDefaultAsync(e => e.Id == request.ExpenseId, ct)
            ?? throw new NotFoundException("Expense", request.ExpenseId);

        db.Expenses.Remove(expense);
        await db.SaveChangesAsync(ct);
    }
}
