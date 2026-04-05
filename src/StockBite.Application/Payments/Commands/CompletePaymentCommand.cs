using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Entities;
using StockBite.Domain.Enums;

namespace StockBite.Application.Payments.Commands;

public record CompletePaymentCommand(string Token) : IRequest<CompletePaymentResult>;
public record CompletePaymentResult(bool Success, string? Message);

public class CompletePaymentCommandHandler(IApplicationDbContext db, IPaymentService paymentService)
    : IRequestHandler<CompletePaymentCommand, CompletePaymentResult>
{
    public async Task<CompletePaymentResult> Handle(CompletePaymentCommand request, CancellationToken ct)
    {
        var payment = await db.Payments.Include(p => p.Package).ThenInclude(p => p.Modules)
            .FirstOrDefaultAsync(p => p.IyzicoToken == request.Token, ct);

        if (payment == null)
            return new CompletePaymentResult(false, "Ödeme kaydı bulunamadı.");

        var result = await paymentService.CompleteCheckoutFormAsync(request.Token, ct);

        if (!result.Success)
        {
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = result.ErrorMessage;
            await db.SaveChangesAsync(ct);
            return new CompletePaymentResult(false, result.ErrorMessage);
        }

        payment.Status = PaymentStatus.Success;
        payment.IyzicoPaymentId = result.PaymentId;

        // Paketteki modülleri tenant'a aktar
        foreach (var module in payment.Package.Modules)
        {
            var existing = await db.TenantModules
                .FirstOrDefaultAsync(tm => tm.TenantId == payment.TenantId && tm.ModuleType == module.ModuleType, ct);

            var expiresAt = payment.Package.DurationDays.HasValue
                ? DateTime.UtcNow.AddDays(payment.Package.DurationDays.Value)
                : (DateTime?)null;

            if (existing != null)
            {
                existing.IsActive = true;
                existing.StartsAt = DateTime.UtcNow;
                existing.ExpiresAt = expiresAt;
            }
            else
            {
                db.TenantModules.Add(new TenantModule
                {
                    TenantId = payment.TenantId,
                    ModuleType = module.ModuleType,
                    IsActive = true,
                    GrantedByAdmin = false,
                    StartsAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt
                });
            }
        }

        await db.SaveChangesAsync(ct);
        return new CompletePaymentResult(true, "Ödeme başarılı. Modüller aktifleştirildi.");
    }
}
