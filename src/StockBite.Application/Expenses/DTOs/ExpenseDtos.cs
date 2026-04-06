namespace StockBite.Application.Expenses.DTOs;

public record ExpenseDto(
    Guid Id,
    decimal Amount,
    int Category,
    string CategoryLabel,
    string Description,
    string Date
);
