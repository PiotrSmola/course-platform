using CoursePlatform.Application.Features.Payments.Queries.GetPaymentHistory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/payments/history")]
[Authorize]
[EnableRateLimiting("api")]
public class PaymentHistoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentHistoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PurchaseHistoryItemDto>>> Get(
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetPaymentHistoryQuery(limit), cancellationToken);
        return Ok(result);
    }
}
