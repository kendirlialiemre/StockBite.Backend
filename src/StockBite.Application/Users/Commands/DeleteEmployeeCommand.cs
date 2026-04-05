using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Enums;

namespace StockBite.Application.Users.Commands;

public record DeleteEmployeeCommand(Guid EmployeeId) : IRequest;

public class DeleteEmployeeCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<DeleteEmployeeCommand>
{
    public async Task Handle(DeleteEmployeeCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException();

        var employee = await db.Users
            .FirstOrDefaultAsync(u => u.Id == request.EmployeeId && u.TenantId == tenantId, ct)
            ?? throw new NotFoundException("User", request.EmployeeId);

        if (employee.Role != UserRole.Employee)
            throw new InvalidOperationException("Sadece çalışanlar silinebilir.");

        db.Users.Remove(employee);
        await db.SaveChangesAsync(ct);
    }
}
