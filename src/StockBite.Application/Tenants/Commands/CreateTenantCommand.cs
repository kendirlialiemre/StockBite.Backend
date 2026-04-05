using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Tenants.DTOs;
using StockBite.Domain.Entities;
using StockBite.Domain.Enums;

namespace StockBite.Application.Tenants.Commands;

public record CreateTenantCommand(
    string Name, string Slug,
    string OwnerEmail, string OwnerFirstName, string OwnerLastName, string OwnerPassword
) : IRequest<TenantDto>;

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100)
            .Matches("^[a-z0-9-]+$").WithMessage("Slug sadece küçük harf, rakam ve tire içerebilir.");
        RuleFor(x => x.OwnerEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.OwnerPassword).NotEmpty().MinimumLength(6);
    }
}

public class CreateTenantCommandHandler(IApplicationDbContext db, IPasswordHasher passwordHasher)
    : IRequestHandler<CreateTenantCommand, TenantDto>
{
    public async Task<TenantDto> Handle(CreateTenantCommand request, CancellationToken ct)
    {
        if (await db.Tenants.AnyAsync(t => t.Slug == request.Slug, ct))
            throw new InvalidOperationException($"'{request.Slug}' slug'ı zaten kullanılıyor.");

        var tenant = new Tenant { Name = request.Name, Slug = request.Slug };
        db.Tenants.Add(tenant);

        var owner = new User
        {
            TenantId = tenant.Id,
            Email = request.OwnerEmail.ToLower(),
            PasswordHash = passwordHasher.Hash(request.OwnerPassword),
            FirstName = request.OwnerFirstName,
            LastName = request.OwnerLastName,
            Role = UserRole.Owner
        };
        db.Users.Add(owner);
        await db.SaveChangesAsync(ct);

        return new TenantDto(tenant.Id, tenant.Name, tenant.Slug, tenant.IsActive, tenant.CreatedAt);
    }
}
