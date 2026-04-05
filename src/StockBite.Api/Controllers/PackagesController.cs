using MediatR;
using Microsoft.AspNetCore.Mvc;
using StockBite.Application.Packages.Queries;

namespace StockBite.Api.Controllers;

[ApiController]
[Route("api/packages")]
public class PackagesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await mediator.Send(new GetPackagesQuery(ActiveOnly: true), ct));
}
