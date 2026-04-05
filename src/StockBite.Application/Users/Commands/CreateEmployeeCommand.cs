using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Users.DTOs;
using StockBite.Domain.Entities;
using StockBite.Domain.Enums;

namespace StockBite.Application.Users.Commands;

public record CreateEmployeeCommand(string Email, string FirstName, string LastName, string Password) : IRequest<EmployeeDto>;

public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
    }
}

public class CreateEmployeeCommandHandler(
    IApplicationDbContext db, IPasswordHasher passwordHasher, ICurrentUserService currentUser)
    : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    public async Task<EmployeeDto> Handle(CreateEmployeeCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException();

        if (await db.Users.AnyAsync(u => u.Email == request.Email.ToLower(), ct))
            throw new InvalidOperationException("Bu email adresi zaten kullanılıyor.");

        var employee = new User
        {
            TenantId = tenantId,
            Email = request.Email.ToLower(),
            PasswordHash = passwordHasher.Hash(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = UserRole.Employee
        };
        db.Users.Add(employee);
        await db.SaveChangesAsync(ct);

        return new EmployeeDto(employee.Id, employee.Email, employee.FirstName,
            employee.LastName, employee.Role.ToString(), employee.IsActive, []);
    }
}
