using CoursePlatform.Application.Features.Trials;
using CoursePlatform.Application.Features.Trials.Commands.StartTrial;
using CoursePlatform.Application.Features.Trials.Queries.GetTrialEligibleCourses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/trials")]
[Authorize]
[EnableRateLimiting("api")]
public class TrialsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TrialsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("eligible-courses")]
    public async Task<ActionResult<IReadOnlyList<TrialEligibleCourseDto>>> GetEligibleCourses(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTrialEligibleCoursesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TrialAccessDto>> Start(
        StartTrialCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
