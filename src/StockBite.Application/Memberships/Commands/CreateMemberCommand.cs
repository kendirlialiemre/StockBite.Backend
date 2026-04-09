using MediatR;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Memberships.DTOs;
using StockBite.Domain.Entities;

namespace StockBite.Application.Memberships.Commands;

public record CreateMemberCommand(string Name, string? Phone, string? Note) : IRequest<MemberDto>;

public class CreateMemberCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<CreateMemberCommand, MemberDto>
{
    public async Task<MemberDto> Handle(CreateMemberCommand request, CancellationToken ct)
    {
        var member = new Member
        {
            TenantId = currentUser.TenantId!.Value,
            Name = request.Name.Trim(),
            Phone = request.Phone?.Trim(),
            Note = request.Note?.Trim(),
        };
        db.Members.Add(member);
        await db.SaveChangesAsync(ct);
        return new MemberDto(member.Id, member.Name, member.Phone, member.Note, 0, 0, member.CreatedAt);
    }
}
