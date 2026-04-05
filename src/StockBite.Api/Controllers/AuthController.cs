using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockBite.Application.Auth.Commands;
using StockBite.Application.Auth.Queries;

namespace StockBite.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct) =>
        Ok(await mediator.Send(command, ct));

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken ct) =>
        Ok(await mediator.Send(command, ct));

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct) =>
        Ok(await mediator.Send(new GetMeQuery(), ct));

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout() => Ok(new { message = "Çıkış yapıldı." });
}
