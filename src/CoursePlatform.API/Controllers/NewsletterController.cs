using CoursePlatform.Application.Features.Newsletter;
using CoursePlatform.Application.Features.Newsletter.Commands.ConfirmNewsletterSubscription;
using CoursePlatform.Application.Features.Newsletter.Commands.SubscribeToNewsletter;
using CoursePlatform.Application.Features.Newsletter.Commands.UnsubscribeFromNewsletter;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/newsletter/subscriptions")]
[AllowAnonymous]
[EnableRateLimiting("newsletter")]
public class NewsletterController : ControllerBase
{
    private readonly IMediator _mediator;

    public NewsletterController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<NewsletterSubscriptionResult>> Subscribe(
        SubscribeToNewsletterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Accepted(result);
    }

    [HttpPost("confirm")]
    public async Task<ActionResult<NewsletterSubscriptionResult>> Confirm(
        ConfirmNewsletterSubscriptionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("unsubscribe")]
    public async Task<ActionResult<NewsletterSubscriptionResult>> Unsubscribe(
        UnsubscribeFromNewsletterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
