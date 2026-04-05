using MediatR;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Orders.DTOs;
using StockBite.Domain.Entities;

namespace StockBite.Application.Orders.Commands;

public record OpenTableCommand(string Name) : IRequest<TableWithOrderDto>;

public class OpenTableCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<OpenTableCommand, TableWithOrderDto>
{
    public async Task<TableWithOrderDto> Handle(OpenTableCommand request, CancellationToken ct)
    {
        var table = new Table
        {
            TenantId = currentUser.TenantId!.Value,
            Name = request.Name,
            Capacity = 0,
            IsActive = true
        };
        db.Tables.Add(table);

        var order = new Order
        {
            TenantId = currentUser.TenantId!.Value,
            TableId = table.Id,
            CreatedBy = currentUser.UserId!.Value
        };
        db.Orders.Add(order);

        await db.SaveChangesAsync(ct);

        return new TableWithOrderDto(table.Id, table.Name, order.Id, 0, 0, order.OpenedAt);
    }
}
