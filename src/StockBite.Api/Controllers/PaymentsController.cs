using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockBite.Application.Payments.Commands;
using StockBite.Application.Payments.Queries;

namespace StockBite.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken ct) =>
        Ok(await mediator.Send(new GetMyPaymentsQuery(), ct));

    [Authorize]
    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate([FromBody] InitiateRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new InitiatePaymentCommand(req.PackageId, req.CallbackUrl), ct);
        return Ok(result);
    }

    // iyzico bu endpoint'i POST ile callback yapar — token form-encoded gelir
    [HttpPost("callback")]
    public async Task<IActionResult> Callback([FromForm] string token, CancellationToken ct)
    {
        var result = await mediator.Send(new CompletePaymentCommand(token), ct);
        // Kullanıcıyı frontend'e yönlendir
        var redirectUrl = result.Success
            ? "http://localhost:3001/payment/success"
            : "http://localhost:3001/payment/failed";
        return Redirect(redirectUrl);
    }
}

public record InitiateRequest(Guid PackageId, string CallbackUrl);
