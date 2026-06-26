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
    public async Task<ActionResult<Guid>> Enroll(EnrollCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<List<EnrollmentDto>>> GetMyEnrollments()
    {
        var result = await _mediator.Send(new GetMyEnrollmentsQuery());
        return Ok(result);
    }
}
