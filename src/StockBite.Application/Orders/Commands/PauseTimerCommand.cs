using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Entities;
using StockBite.Domain.Enums;

namespace StockBite.Application.Orders.Commands;

public record PauseTimerCommand(Guid OrderId) : IRequest;

public class PauseTimerCommandHandler(IApplicationDbContext db)
    : IRequestHandler<PauseTimerCommand>
{
    public async Task Handle(PauseTimerCommand request, CancellationToken ct)
    {
        var order = await db.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.Status == OrderStatus.Open, ct)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        if (order.IsTimerPaused) return;

        var elapsed = (int)(DateTime.UtcNow - order.TimerLastStartedAt).TotalSeconds;
        order.TimerOffsetSeconds += elapsed;
        order.IsTimerPaused = true;

        await db.SaveChangesAsync(ct);
    }
}
