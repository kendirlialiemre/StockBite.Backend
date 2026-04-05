using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Users.DTOs;

namespace StockBite.Application.Users.Queries;

public record GetEmployeesQuery : IRequest<List<EmployeeDto>>;

public class GetEmployeesQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GetEmployeesQuery, List<EmployeeDto>>
{
    public async Task<List<EmployeeDto>> Handle(GetEmployeesQuery request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException();
        return await db.Users
            .Include(u => u.Permissions)
            .Where(u => u.TenantId == tenantId)
            .Select(u => new EmployeeDto(u.Id, u.Email, u.FirstName, u.LastName,
                u.Role.ToString(), u.IsActive,
                u.Permissions.Select(p => p.Permission).ToList()))
            .ToListAsync(ct);
    }
}
