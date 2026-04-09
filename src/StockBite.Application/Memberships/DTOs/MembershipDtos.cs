namespace StockBite.Application.Memberships.DTOs;

public record MemberDto(
    Guid Id,
    string Name,
    string? Phone,
    string? Note,
    int SubscriptionCount,
    decimal TotalRemainingHours,
    DateTime CreatedAt
);

public record MemberDetailDto(
    Guid Id,
    string Name,
    string? Phone,
    string? Note,
    DateTime CreatedAt,
    List<SubscriptionDto> Subscriptions
);

public record SubscriptionDto(
    Guid Id,
    decimal TotalHours,
    decimal RemainingHours,
    decimal Price,
    string? Note,
    DateTime PurchasedAt,
    List<SessionDto> Sessions
);

public record SessionDto(
    Guid Id,
    decimal Hours,
    string? Note,
    DateTime SessionAt
);
