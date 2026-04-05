namespace StockBite.Application.Common.Interfaces;
public interface IPaymentService
{
    Task<InitiatePaymentResult> InitiateCheckoutFormAsync(InitiatePaymentRequest request, CancellationToken ct = default);
    Task<CompletePaymentResult> CompleteCheckoutFormAsync(string token, CancellationToken ct = default);
}

public record InitiatePaymentRequest(
    Guid PaymentId,
    string ConversationId,
    decimal Price,
    string PackageName,
    string BuyerEmail,
    string BuyerFirstName,
    string BuyerLastName,
    string CallbackUrl
);

public record InitiatePaymentResult(bool Success, string? CheckoutFormContent, string? Token, string? ErrorMessage);
public record CompletePaymentResult(bool Success, string? PaymentId, string? ConversationId, string? ErrorMessage);
