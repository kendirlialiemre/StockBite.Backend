using StockBite.Domain.Enums;

namespace StockBite.Application.Orders.DTOs;

public record TableDto(Guid Id, string Name, int Capacity, bool IsActive);
public record TableWithOrderDto(Guid Id, string Name, Guid? OrderId, decimal Total, int ItemCount, DateTime OpenedAt, bool IsTimerPaused, int TimerOffsetSeconds, DateTime TimerLastStartedAt);
public record OrderItemDto(Guid Id, Guid MenuItemId, string MenuItemName, int Quantity, decimal UnitPrice, string? Note);
public record OrderDto(Guid Id, Guid? TableId, string? TableName, OrderStatus Status, DateTime OpenedAt, DateTime? ClosedAt, decimal TotalAmount, string? Note, PaymentMethod? PaymentMethod, decimal? CashAmount, decimal? CardAmount, IReadOnlyList<OrderItemDto> Items);
