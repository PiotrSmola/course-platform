using CoursePlatform.Application.Features.Courses.Queries.GetCourses;
using CoursePlatform.Application.Features.Courses.Queries.GetPopularCourses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/courses/popular")]
[EnableRateLimiting("api")]
public class PopularCoursesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PopularCoursesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<CourseListDto>>> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPopularCoursesQuery(), cancellationToken);
        return Ok(result);
    }
}
