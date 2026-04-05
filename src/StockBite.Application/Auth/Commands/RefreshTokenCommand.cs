using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Enums;

namespace StockBite.Application.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<LoginResult>;

public class RefreshTokenCommandHandler(
    IApplicationDbContext db,
    IJwtTokenService jwtTokenService) : IRequestHandler<RefreshTokenCommand, LoginResult>
{
    public async Task<LoginResult> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var storedToken = await db.RefreshTokens
            .Include(rt => rt.User).ThenInclude(u => u.Permissions)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, ct)
            ?? throw new ForbiddenException("Geçersiz refresh token.");

        if (!storedToken.IsActive)
            throw new ForbiddenException("Refresh token süresi dolmuş veya iptal edilmiş.");

        storedToken.RevokedAt = DateTime.UtcNow;

        var user = storedToken.User;
        var permissions = user.Role is UserRole.Owner or UserRole.SuperAdmin
            ? new List<string>()
            : user.Permissions.Select(p => p.Permission).ToList();

        var newAccessToken = jwtTokenService.GenerateAccessToken(user, permissions);
        var newRefreshValue = jwtTokenService.GenerateRefreshToken();

        var newRefreshToken = new Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshValue,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        db.RefreshTokens.Add(newRefreshToken);
        await db.SaveChangesAsync(ct);

        return new LoginResult(
            newAccessToken,
            newRefreshValue,
            new UserDto(user.Id, user.Email, user.FirstName, user.LastName,
                user.Role.ToString(), user.TenantId, permissions));
    }
}
