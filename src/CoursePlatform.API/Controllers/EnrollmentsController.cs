using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoursePlatform.Application.Features.Enrollments.Commands.Enroll;
using CoursePlatform.Application.Features.Enrollments.Queries.GetMyEnrollments;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("api")]
public class EnrollmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EnrollmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Guid>> Enroll(EnrollCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<List<EnrollmentDto>>> GetMyEnrollments(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyEnrollmentsQuery(), cancellationToken);
        return Ok(result);
    }
}
