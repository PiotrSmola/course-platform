using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoursePlatform.Application.Features.LessonDiscussions.Commands.CreateLessonAnswer;
using CoursePlatform.Application.Features.LessonDiscussions.Commands.CreateLessonQuestion;
using CoursePlatform.Application.Features.LessonDiscussions.Commands.DeleteLessonAnswer;
using CoursePlatform.Application.Features.LessonDiscussions.Commands.DeleteLessonQuestion;
using CoursePlatform.Application.Features.LessonDiscussions.Queries.GetLessonDiscussion;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/courses/{courseId:guid}/lessons/{lessonId:guid}/discussion")]
[Authorize]
[EnableRateLimiting("api")]
public class LessonDiscussionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LessonDiscussionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<LessonDiscussionDto>> GetDiscussion(
        Guid courseId,
        Guid lessonId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetLessonDiscussionQuery(courseId, lessonId, pageNumber, pageSize),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("questions")]
    public async Task<ActionResult<Guid>> CreateQuestion(
        Guid courseId,
        Guid lessonId,
        [FromBody] CreateLessonQuestionRequest body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateLessonQuestionCommand(courseId, lessonId, body.Body),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("questions/{questionId:guid}/answers")]
    public async Task<ActionResult<Guid>> CreateAnswer(
        Guid courseId,
        Guid lessonId,
        Guid questionId,
        [FromBody] CreateLessonAnswerRequest body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateLessonAnswerCommand(courseId, lessonId, questionId, body.Body),
            cancellationToken);
        return Ok(result);
    }

    [HttpDelete("questions/{questionId:guid}")]
    public async Task<ActionResult> DeleteQuestion(
        Guid courseId,
        Guid lessonId,
        Guid questionId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeleteLessonQuestionCommand(courseId, lessonId, questionId),
            cancellationToken);
        return NoContent();
    }

    [HttpDelete("questions/{questionId:guid}/answers/{answerId:guid}")]
    public async Task<ActionResult> DeleteAnswer(
        Guid courseId,
        Guid lessonId,
        Guid questionId,
        Guid answerId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeleteLessonAnswerCommand(courseId, lessonId, questionId, answerId),
            cancellationToken);
        return NoContent();
    }
}

public record CreateLessonQuestionRequest(string Body);
public record CreateLessonAnswerRequest(string Body);
