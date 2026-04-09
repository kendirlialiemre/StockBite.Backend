using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Entities;

namespace StockBite.Application.Memberships.Commands;

public record DeleteMemberCommand(Guid MemberId) : IRequest;

public class DeleteMemberCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteMemberCommand>
{
    public async Task Handle(DeleteMemberCommand request, CancellationToken ct)
    {
        var member = await db.Members.FirstOrDefaultAsync(m => m.Id == request.MemberId, ct)
            ?? throw new NotFoundException(nameof(Member), request.MemberId);

        db.Members.Remove(member);
        await db.SaveChangesAsync(ct);
    }
}
