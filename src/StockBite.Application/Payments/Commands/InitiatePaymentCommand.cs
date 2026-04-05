using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Entities;

namespace StockBite.Application.Payments.Commands;

public record InitiatePaymentCommand(Guid PackageId, string CallbackUrl) : IRequest<InitiatePaymentResult>;
public record InitiatePaymentResult(string CheckoutFormContent, string Token);

public class InitiatePaymentCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IPaymentService paymentService)
    : IRequestHandler<InitiatePaymentCommand, InitiatePaymentResult>
{
    public async Task<InitiatePaymentResult> Handle(InitiatePaymentCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException();
        var userId = currentUser.UserId ?? throw new ForbiddenException();

        var package = await db.Packages.FirstOrDefaultAsync(p => p.Id == request.PackageId && p.IsActive, ct)
            ?? throw new NotFoundException(nameof(Package), request.PackageId);

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new NotFoundException(nameof(User), userId);

        var conversationId = Guid.NewGuid().ToString("N");

        var payment = new Payment
        {
            TenantId = tenantId,
            PackageId = package.Id,
            UserId = userId,
            Amount = package.Price,
            ConversationId = conversationId
        };
        db.Payments.Add(payment);
        await db.SaveChangesAsync(ct);

        var result = await paymentService.InitiateCheckoutFormAsync(new(
            payment.Id, conversationId, package.Price, package.Name,
            user.Email, user.FirstName, user.LastName, request.CallbackUrl), ct);

        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage ?? "Ödeme başlatılamadı.");

        payment.IyzicoToken = result.Token;
        await db.SaveChangesAsync(ct);

        return new InitiatePaymentResult(result.CheckoutFormContent!, result.Token!);
    }
}
