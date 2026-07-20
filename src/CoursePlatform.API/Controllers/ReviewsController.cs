using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoursePlatform.Application.Features.Reviews.Commands.CreateReview;
using CoursePlatform.Application.Features.Reviews.Commands.DeleteOwnReview;
using CoursePlatform.Application.Features.Reviews.Commands.UpdateReview;
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
    public async Task<ActionResult<List<ReviewListDto>>> GetCourseReviews(Guid courseId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCourseReviewsQuery(courseId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Guid>> CreateReview(Guid courseId, CreateReviewCommand command, CancellationToken cancellationToken)
    {
        if (courseId != command.CourseId) return BadRequest();
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{reviewId:guid}")]
    [Authorize]
    public async Task<ActionResult> UpdateReview(Guid courseId, Guid reviewId, UpdateReviewCommand command, CancellationToken cancellationToken)
    {
        if (courseId != command.CourseId || reviewId != command.ReviewId) return BadRequest();
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{reviewId:guid}")]
    [Authorize]
    public async Task<ActionResult> DeleteReview(Guid courseId, Guid reviewId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteOwnReviewCommand(courseId, reviewId), cancellationToken);
        return NoContent();
    }
}
