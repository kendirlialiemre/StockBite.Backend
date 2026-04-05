using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;

namespace StockBite.Application.Users.Commands;

public record UpdateProfileCommand(
    string FirstName,
    string LastName,
    string? CurrentPassword,
    string? NewPassword
) : IRequest;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        When(x => x.NewPassword != null, () =>
        {
            RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Mevcut şifre gereklidir.");
            RuleFor(x => x.NewPassword).MinimumLength(8).WithMessage("Yeni şifre en az 8 karakter olmalıdır.");
        });
    }
}

public class UpdateProfileCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IPasswordHasher passwordHasher)
    : IRequestHandler<UpdateProfileCommand>
{
    public async Task Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new ForbiddenException();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new NotFoundException("User", userId);

        if (request.NewPassword != null)
        {
            if (!passwordHasher.Verify(request.CurrentPassword!, user.PasswordHash))
                throw new InvalidOperationException("Mevcut şifre hatalı.");

            user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

        await db.SaveChangesAsync(ct);
    }
}
