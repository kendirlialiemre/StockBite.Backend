using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockBite.Application.Public;

namespace StockBite.Api.Controllers;

[ApiController]
[Route("api/public")]
public class PublicController(IMediator mediator) : ControllerBase
{
    [HttpGet("menu/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicMenu(string slug, [FromQuery] Guid? @ref, CancellationToken ct)
    {
        var result = await mediator.Send(new GetPublicMenuQuery(slug, @ref), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("qr-template")]
    [Authorize]
    public async Task<IActionResult> SaveQrTemplate([FromBody] SaveQrTemplateRequest req, CancellationToken ct)
    {
        await mediator.Send(new SaveQrTemplateCommand(req.Template), ct);
        return NoContent();
    }
}

public record SaveQrTemplateRequest(int Template);
