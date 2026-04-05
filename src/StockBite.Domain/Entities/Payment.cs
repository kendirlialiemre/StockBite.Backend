using StockBite.Domain.Enums;
namespace StockBite.Domain.Entities;
public class Payment : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid PackageId { get; set; }
    public Package Package { get; set; } = null!;
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? IyzicoToken { get; set; }          // checkoutFormInitialize token
    public string? IyzicoPaymentId { get; set; }      // success callback paymentId
    public string? FailureReason { get; set; }
    public string? ConversationId { get; set; }        // unique per attempt
}
