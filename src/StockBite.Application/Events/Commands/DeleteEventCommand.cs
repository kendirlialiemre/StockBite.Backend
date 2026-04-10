using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;

namespace StockBite.Application.Events.Commands;

public record DeleteEventCommand(Guid Id) : IRequest;

public class DeleteEventCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteEventCommand>
{
    public async Task Handle(DeleteEventCommand request, CancellationToken ct)
    {
        var ev = await db.Events.FirstOrDefaultAsync(e => e.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Event), request.Id);
        db.Events.Remove(ev);
        await db.SaveChangesAsync(ct);
    }
}
