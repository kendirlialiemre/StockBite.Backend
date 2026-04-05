using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockBite.Application.Users.Commands;
using StockBite.Application.Users.Queries;

namespace StockBite.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Owner")]
public class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetEmployees(CancellationToken ct) =>
        Ok(await mediator.Send(new GetEmployeesQuery(), ct));

    [HttpPost]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeCommand command, CancellationToken ct) =>
        Ok(await mediator.Send(command, ct));

    [HttpPut("{userId:guid}/permissions")]
    public async Task<IActionResult> UpdatePermissions(Guid userId, [FromBody] UpdatePermissionsRequest req, CancellationToken ct)
    {
        await mediator.Send(new UpdatePermissionsCommand(userId, req.Permissions), ct);
        return NoContent();
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> DeleteEmployee(Guid userId, CancellationToken ct)
    {
        await mediator.Send(new DeleteEmployeeCommand(userId), ct);
        return NoContent();
    }
}

public record UpdatePermissionsRequest(IReadOnlyList<string> Permissions);
