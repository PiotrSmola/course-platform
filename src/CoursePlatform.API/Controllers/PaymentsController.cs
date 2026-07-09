using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoursePlatform.Application.Features.Payments.Commands.CreateCheckoutSession;
using CoursePlatform.Application.Features.Payments.Commands.ProcessPaymentWebhook;
using CoursePlatform.Application.Features.Payments.Queries.GetMyPurchases;
using CoursePlatform.Application.Features.Payments.Queries.GetPaymentStatus;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/payments")]
[EnableRateLimiting("api")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("checkout")]
    [Authorize]
    public async Task<ActionResult<CheckoutSessionDto>> CreateCheckoutSession(
        CreateCheckoutSessionCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<List<PurchaseDto>>> GetMyPurchases(
        [FromQuery] int limit = 5, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetMyPurchasesQuery(limit), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{sessionId}")]
    [Authorize]
    public async Task<ActionResult<PaymentStatusDto>> GetPaymentStatus(
        string sessionId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPaymentStatusQuery(sessionId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    [DisableRateLimiting]
    [Consumes("application/json")]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var signature = Request.Headers["Stripe-Signature"].ToString();

        await _mediator.Send(new ProcessPaymentWebhookCommand(payload, signature), cancellationToken);
        return Ok();
    }
}
