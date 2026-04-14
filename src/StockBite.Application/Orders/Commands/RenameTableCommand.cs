using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Entities;

namespace StockBite.Application.Orders.Commands;

public record RenameTableCommand(Guid TableId, string Name) : IRequest;

public class RenameTableCommandHandler(IApplicationDbContext db)
    : IRequestHandler<RenameTableCommand>
{
    public async Task Handle(RenameTableCommand request, CancellationToken ct)
    {
        var table = await db.Tables.FirstOrDefaultAsync(t => t.Id == request.TableId, ct)
            ?? throw new NotFoundException(nameof(Table), request.TableId);

        table.Name = request.Name.Trim();
        await db.SaveChangesAsync(ct);
    }
}
