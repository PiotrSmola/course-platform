using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoursePlatform.Application.Common.Authorization;
using CoursePlatform.Application.Features.Quizzes;
using CoursePlatform.Application.Features.Quizzes.Commands.DeleteQuiz;
using CoursePlatform.Application.Features.Quizzes.Commands.SubmitQuizAttempt;
using CoursePlatform.Application.Features.Quizzes.Commands.UpsertQuiz;
using CoursePlatform.Application.Features.Quizzes.Queries.GetLessonQuiz;
using CoursePlatform.Application.Features.Quizzes.Queries.GetLessonQuizForInstructor;
using CoursePlatform.Application.Features.Quizzes.Queries.GetMyQuizAttempts;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/courses/{courseId:guid}/lessons/{lessonId:guid}/quiz")]
[Authorize]
[EnableRateLimiting("api")]
public class QuizzesController : ControllerBase
{
    private readonly IMediator _mediator;

    public QuizzesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<LessonQuizStudentDto?>> GetQuiz(
        Guid courseId,
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLessonQuizQuery(courseId, lessonId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("manage")]
    [Authorize(Policy = AuthorizationPolicies.InstructorOrAdmin)]
    public async Task<ActionResult<LessonQuizInstructorDto?>> GetQuizForInstructor(
        Guid courseId,
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetLessonQuizForInstructorQuery(courseId, lessonId),
            cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    [Authorize(Policy = AuthorizationPolicies.InstructorOrAdmin)]
    public async Task<ActionResult<Guid>> UpsertQuiz(
        Guid courseId,
        Guid lessonId,
        [FromBody] UpsertQuizRequest body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpsertQuizCommand(
                courseId,
                lessonId,
                body.Title,
                body.PassThresholdPercent,
                body.Questions),
            cancellationToken);
        return Ok(result);
    }

    [HttpDelete]
    [Authorize(Policy = AuthorizationPolicies.InstructorOrAdmin)]
    public async Task<ActionResult> DeleteQuiz(
        Guid courseId,
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteQuizCommand(courseId, lessonId), cancellationToken);
        return NoContent();
    }

    [HttpPost("attempts")]
    public async Task<ActionResult<QuizAttemptResultDto>> SubmitAttempt(
        Guid courseId,
        Guid lessonId,
        [FromBody] SubmitQuizAttemptRequest body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SubmitQuizAttemptCommand(courseId, lessonId, body.Answers),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("attempts/me")]
    public async Task<ActionResult<IReadOnlyList<QuizAttemptListItemDto>>> GetMyAttempts(
        Guid courseId,
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetMyQuizAttemptsQuery(courseId, lessonId),
            cancellationToken);
        return Ok(result);
    }
}

public record SubmitQuizAttemptRequest(IReadOnlyList<QuizAttemptAnswerInputDto> Answers);
