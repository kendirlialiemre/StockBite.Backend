using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Entities;
using StockBite.Domain.Enums;

namespace StockBite.Application.Auth.Commands;

public record RegisterCommand(
    string TenantName,
    string Slug,
    string Email,
    string FirstName,
    string LastName,
    string Password
) : IRequest<LoginResult>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.TenantName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100)
            .Matches("^[a-z0-9-]+$").WithMessage("Slug sadece küçük harf, rakam ve tire içerebilir.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
    }
}

public class RegisterCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IRequestHandler<RegisterCommand, LoginResult>
{
    public async Task<LoginResult> Handle(RegisterCommand request, CancellationToken ct)
    {
        if (await db.Tenants.AnyAsync(t => t.Slug == request.Slug, ct))
            throw new InvalidOperationException($"'{request.Slug}' slug'ı zaten kullanılıyor.");

        if (await db.Users.AnyAsync(u => u.Email == request.Email.ToLower(), ct))
            throw new InvalidOperationException("Bu email adresi zaten kayıtlı.");

        var tenant = new Tenant { Name = request.TenantName, Slug = request.Slug };
        db.Tenants.Add(tenant);

        var owner = new User
        {
            TenantId = tenant.Id,
            Email = request.Email.ToLower(),
            PasswordHash = passwordHasher.Hash(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = UserRole.Owner
        };
        db.Users.Add(owner);
        await db.SaveChangesAsync(ct);

        var accessToken = jwtTokenService.GenerateAccessToken(owner, []);
        var refreshTokenValue = jwtTokenService.GenerateRefreshToken();

        db.RefreshTokens.Add(new Domain.Entities.RefreshToken
        {
            UserId = owner.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        });
        await db.SaveChangesAsync(ct);

        return new LoginResult(accessToken, refreshTokenValue,
            new UserDto(owner.Id, owner.Email, owner.FirstName, owner.LastName, owner.Role.ToString(), tenant.Id, []));
    }
}
