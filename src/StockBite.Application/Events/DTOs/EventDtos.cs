using StockBite.Domain.Enums;

namespace StockBite.Application.Events.DTOs;

public record EventDto(
    Guid Id,
    string PersonName,
    int? Age,
    DateOnly EventDate,
    int AdultCount,
    int ChildCount,
    string EventType,
    string? Package,
    decimal ChargedAmount,
    decimal Cost,
    decimal Profit,
    string? Notes,
    EventStatus Status,
    DateTime CreatedAt,
    PaymentMethod? PaymentMethod,
    decimal CashAmount,
    decimal CardAmount,
    DateTime? PaidAt
);
