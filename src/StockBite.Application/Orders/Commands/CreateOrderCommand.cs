using MediatR;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Orders.DTOs;
using StockBite.Domain.Entities;

namespace StockBite.Application.Orders.Commands;

public record CreateOrderCommand(Guid? TableId, string? Note) : IRequest<OrderDto>;

public class CreateOrderCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<CreateOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var order = new Order
        {
            TenantId = currentUser.TenantId!.Value,
            TableId = request.TableId,
            Note = request.Note,
            CreatedBy = currentUser.UserId!.Value
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);
        return new OrderDto(order.Id, order.TableId, null, order.Status,
            order.OpenedAt, order.ClosedAt, order.TotalAmount, order.Note, order.PaymentMethod, []);
    }
}

