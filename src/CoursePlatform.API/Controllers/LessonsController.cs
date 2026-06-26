using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoursePlatform.Application.Features.Lessons.Queries.GetLesson;
using CoursePlatform.Application.Features.Lessons.Commands.UpdateProgress;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/courses/{courseId:guid}/lessons")]
[EnableRateLimiting("api")]
public class LessonsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LessonsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{lessonId:guid}")]
    [Authorize]
    public async Task<ActionResult<LessonDto>> GetLesson(Guid courseId, Guid lessonId)
    {
        var result = await _mediator.Send(new GetLessonQuery(courseId, lessonId));
        return Ok(result);
    }

    [HttpPost("{lessonId:guid}/complete")]
    [Authorize]
    public async Task<ActionResult> CompleteLesson(Guid courseId, Guid lessonId)
    {
        await _mediator.Send(new UpdateProgressCommand(courseId, lessonId));
        return NoContent();
    }
}
