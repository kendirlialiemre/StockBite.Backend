using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Entities;
using StockBite.Domain.Enums;

namespace StockBite.Application.Orders.Commands;

public record ResumeTimerCommand(Guid OrderId) : IRequest;

public class ResumeTimerCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ResumeTimerCommand>
{
    public async Task Handle(ResumeTimerCommand request, CancellationToken ct)
    {
        var order = await db.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.Status == OrderStatus.Open, ct)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        if (!order.IsTimerPaused) return;

        order.IsTimerPaused = false;
        order.TimerLastStartedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
