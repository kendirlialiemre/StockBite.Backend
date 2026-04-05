using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Options;
using StockBite.Application.Common.Interfaces;

namespace StockBite.Infrastructure.Payment;

public class IyzicoPaymentService(IOptions<IyzicoSettings> options) : IPaymentService
{
    private readonly Iyzipay.Options _iyzicoOptions = new()
    {
        ApiKey = options.Value.ApiKey,
        SecretKey = options.Value.SecretKey,
        BaseUrl = options.Value.BaseUrl
    };

    public async Task<InitiatePaymentResult> InitiateCheckoutFormAsync(InitiatePaymentRequest request, CancellationToken ct = default)
    {
        var initializeRequest = new CreateCheckoutFormInitializeRequest
        {
            Locale = Locale.TR.ToString(),
            ConversationId = request.ConversationId,
            Price = request.Price.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
            PaidPrice = request.Price.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
            Currency = Currency.TRY.ToString(),
            BasketId = request.PaymentId.ToString(),
            PaymentGroup = PaymentGroup.PRODUCT.ToString(),
            CallbackUrl = request.CallbackUrl,
            EnabledInstallments = new List<int> { 1, 2, 3, 6, 9, 12 },
            Buyer = new Buyer
            {
                Id = request.ConversationId,
                Name = request.BuyerFirstName,
                Surname = request.BuyerLastName,
                Email = request.BuyerEmail,
                IdentityNumber = "74300864791",
                RegistrationAddress = "Türkiye",
                City = "Istanbul",
                Country = "Turkey",
                Ip = "85.34.78.112"
            },
            ShippingAddress = new Address
            {
                ContactName = $"{request.BuyerFirstName} {request.BuyerLastName}",
                City = "Istanbul",
                Country = "Turkey",
                Description = "Dijital ürün"
            },
            BillingAddress = new Address
            {
                ContactName = $"{request.BuyerFirstName} {request.BuyerLastName}",
                City = "Istanbul",
                Country = "Turkey",
                Description = "Dijital ürün"
            },
            BasketItems = new List<BasketItem>
            {
                new()
                {
                    Id = request.PaymentId.ToString(),
                    Name = request.PackageName,
                    Category1 = "SaaS",
                    ItemType = BasketItemType.VIRTUAL.ToString(),
                    Price = request.Price.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
                }
            }
        };

        var form = await Task.Run(() => CheckoutFormInitialize.Create(initializeRequest, _iyzicoOptions), ct);

        if (form.Status != "success")
            return new InitiatePaymentResult(false, null, null, form.ErrorMessage);

        return new InitiatePaymentResult(true, form.CheckoutFormContent, form.Token, null);
    }

    public async Task<CompletePaymentResult> CompleteCheckoutFormAsync(string token, CancellationToken ct = default)
    {
        var request = new RetrieveCheckoutFormRequest { Token = token };
        var result = await Task.Run(() => CheckoutForm.Retrieve(request, _iyzicoOptions), ct);

        if (result.Status != "success" || result.PaymentStatus != "SUCCESS")
            return new CompletePaymentResult(false, null, null, result.ErrorMessage ?? result.PaymentStatus);

        return new CompletePaymentResult(true, result.PaymentId, result.ConversationId, null);
    }
}
