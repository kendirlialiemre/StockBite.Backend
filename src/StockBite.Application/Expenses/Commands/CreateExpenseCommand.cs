using FluentValidation;
using MediatR;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Expenses.DTOs;
using StockBite.Domain.Entities;
using StockBite.Domain.Enums;

namespace StockBite.Application.Expenses.Commands;

public record CreateExpenseCommand(
    decimal Amount,
    ExpenseCategory Category,
    string Description,
    DateOnly Date
) : IRequest<ExpenseDto>;

public class CreateExpenseCommandValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Tutar 0'dan büyük olmalıdır.");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
    }
}

public class CreateExpenseCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<CreateExpenseCommand, ExpenseDto>
{
    public async Task<ExpenseDto> Handle(CreateExpenseCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException();

        var expense = new Expense
        {
            TenantId = tenantId,
            Amount = request.Amount,
            Category = request.Category,
            Description = request.Description,
            Date = request.Date,
        };

        db.Expenses.Add(expense);
        await db.SaveChangesAsync(ct);

        return ToDto(expense);
    }

    public static ExpenseDto ToDto(Expense e) => new(
        e.Id,
        e.Amount,
        (int)e.Category,
        CategoryLabel(e.Category),
        e.Description,
        e.Date.ToString("yyyy-MM-dd")
    );

    public static string CategoryLabel(ExpenseCategory cat) => cat switch
    {
        ExpenseCategory.Salary => "Personel / Maaş",
        ExpenseCategory.Rent => "Kira",
        ExpenseCategory.Maintenance => "Bakım / Onarım",
        ExpenseCategory.Utilities => "Faturalar",
        _ => "Diğer"
    };
}
