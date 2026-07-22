using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoursePlatform.Application.Common.Authorization;
using CoursePlatform.Application.Features.LessonDiscussions.Queries.GetModerationQueue;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/moderation")]
[Authorize(Policy = AuthorizationPolicies.InstructorOrAdmin)]
[EnableRateLimiting("api")]
public class ModerationController : ControllerBase
{
    private readonly IMediator _mediator;

    public ModerationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("qa")]
    public async Task<ActionResult<ModerationQueueDto>> GetQaQueue(
        [FromQuery] Guid? courseId,
        [FromQuery] bool unansweredByInstructorOnly = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetModerationQueueQuery(courseId, unansweredByInstructorOnly, pageNumber, pageSize),
            cancellationToken);
        return Ok(result);
    }
}
