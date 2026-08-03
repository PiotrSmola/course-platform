using CoursePlatform.Application.Features.Support.Commands.SubmitSupportMessage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/support/messages")]
[AllowAnonymous]
[EnableRateLimiting("contact")]
public class SupportController : ControllerBase
{
    private readonly IMediator _mediator;

    public SupportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult> Submit(
        SubmitSupportMessageCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
