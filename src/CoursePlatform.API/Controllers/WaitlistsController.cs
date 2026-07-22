using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoursePlatform.Application.Features.Waitlists.Commands.JoinWaitlist;
using CoursePlatform.Application.Features.Waitlists.Commands.LeaveWaitlist;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("api")]
public class WaitlistsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WaitlistsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("{courseId:guid}")]
    [Authorize]
    public async Task<ActionResult<Guid>> JoinWaitlist(Guid courseId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new JoinWaitlistCommand(courseId), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{courseId:guid}")]
    [Authorize]
    public async Task<IActionResult> LeaveWaitlist(Guid courseId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new LeaveWaitlistCommand(courseId), cancellationToken);
        return NoContent();
    }
}
