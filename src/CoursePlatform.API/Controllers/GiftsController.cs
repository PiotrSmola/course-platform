using CoursePlatform.Application.Features.Gifts;
using CoursePlatform.Application.Features.Gifts.Commands.CreateGiftCheckout;
using CoursePlatform.Application.Features.Gifts.Commands.RedeemGiftCode;
using CoursePlatform.Application.Features.Gifts.Commands.ResendGiftEmail;
using CoursePlatform.Application.Features.Gifts.Queries.GetMyGifts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/gifts")]
[Authorize]
[EnableRateLimiting("api")]
public class GiftsController : ControllerBase
{
    private readonly IMediator _mediator;

    public GiftsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<GiftCheckoutSessionDto>> Checkout(
        CreateGiftCheckoutCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("redeem")]
    public async Task<ActionResult<GiftRedemptionDto>> Redeem(
        RedeemGiftCodeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<GiftPurchaseListItemDto>>> GetMy(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyGiftsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{giftId:guid}/resend")]
    public async Task<ActionResult> Resend(Guid giftId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ResendGiftEmailCommand(giftId), cancellationToken);
        return NoContent();
    }
}
