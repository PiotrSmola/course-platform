using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoursePlatform.Application.Features.Reviews.Commands.CreateReview;
using CoursePlatform.Application.Features.Reviews.Queries.GetCourseReviews;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/courses/{courseId:guid}/reviews")]
[EnableRateLimiting("api")]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<ReviewListDto>>> GetCourseReviews(Guid courseId)
    {
        var result = await _mediator.Send(new GetCourseReviewsQuery(courseId));
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Guid>> CreateReview(Guid courseId, CreateReviewCommand command)
    {
        if (courseId != command.CourseId) return BadRequest();
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
