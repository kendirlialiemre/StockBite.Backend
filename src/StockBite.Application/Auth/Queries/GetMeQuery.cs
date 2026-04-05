using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Auth.Commands;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Enums;

namespace StockBite.Application.Auth.Queries;

public record GetMeQuery : IRequest<UserDto>;

public class GetMeQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GetMeQuery, UserDto>
{
    public async Task<UserDto> Handle(GetMeQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new ForbiddenException();
        var user = await db.Users.Include(u => u.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), userId);

        var permissions = user.Role is UserRole.Owner or UserRole.SuperAdmin
            ? new List<string>()
            : user.Permissions.Select(p => p.Permission).ToList();

        return new UserDto(user.Id, user.Email, user.FirstName, user.LastName,
            user.Role.ToString(), user.TenantId, permissions);
    }
}
