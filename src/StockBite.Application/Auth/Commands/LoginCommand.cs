using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Enums;

namespace StockBite.Application.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

public record LoginResult(string AccessToken, string RefreshToken, UserDto User);

public record UserDto(Guid Id, string Email, string FirstName, string LastName, string Role, Guid? TenantId, IReadOnlyList<string> Permissions);

public class LoginCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await db.Users
            .Include(u => u.Permissions)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower() && u.IsActive, ct)
            ?? throw new ForbiddenException("Email veya şifre hatalı.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new ForbiddenException("Email veya şifre hatalı.");

        var permissions = user.Role is UserRole.Owner or UserRole.SuperAdmin
            ? new List<string>()
            : user.Permissions.Select(p => p.Permission).ToList();

        var accessToken = jwtTokenService.GenerateAccessToken(user, permissions);
        var refreshTokenValue = jwtTokenService.GenerateRefreshToken();

        var refreshToken = new Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync(ct);

        return new LoginResult(
            accessToken,
            refreshTokenValue,
            new UserDto(user.Id, user.Email, user.FirstName, user.LastName,
                user.Role.ToString(), user.TenantId, permissions));
    }
}
