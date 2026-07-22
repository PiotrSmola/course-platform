using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoursePlatform.Application.Features.Lessons.Queries.GetLesson;
using CoursePlatform.Application.Features.Lessons.Commands.UpdateLessonWatchPosition;
using CoursePlatform.Application.Features.Lessons.Commands.UpdateProgress;
using CoursePlatform.Application.Features.Lessons.Queries.GetLessonVideoUrl;
using CoursePlatform.Application.Features.Lessons.Commands.VideoUploads;

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
    public async Task<ActionResult<LessonDto>> GetLesson(Guid courseId, Guid lessonId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLessonQuery(courseId, lessonId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{lessonId:guid}/complete")]
    [Authorize]
    public async Task<ActionResult> CompleteLesson(Guid courseId, Guid lessonId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateProgressCommand(courseId, lessonId), cancellationToken);
        return NoContent();
    }

    [HttpPut("{lessonId:guid}/position")]
    [Authorize]
    public async Task<ActionResult> UpdateWatchPosition(
        Guid courseId,
        Guid lessonId,
        [FromBody] UpdateWatchPositionRequest body,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateLessonWatchPositionCommand(courseId, lessonId, body.PositionSeconds),
            cancellationToken);
        return NoContent();
    }

    [HttpGet("{lessonId:guid}/video")]
    [Authorize]
    public async Task<ActionResult<LessonVideoUrlDto>> GetLessonVideoUrl(Guid courseId, Guid lessonId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLessonVideoUrlQuery(courseId, lessonId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{lessonId:guid}/video/uploads")]
    [Authorize(Policy = CoursePlatform.Application.Common.Authorization.AuthorizationPolicies.InstructorOrAdmin)]
    public async Task<ActionResult<InitiateLessonVideoUploadResult>> InitiateVideoUpload(
        Guid courseId,
        Guid lessonId,
        [FromBody] InitiateLessonVideoUploadCommand command,
        CancellationToken cancellationToken)
    {
        if (courseId != command.CourseId || lessonId != command.LessonId) return BadRequest();
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{lessonId:guid}/video/uploads/{uploadId}/parts/presign")]
    [Authorize(Policy = CoursePlatform.Application.Common.Authorization.AuthorizationPolicies.InstructorOrAdmin)]
    public async Task<ActionResult<PresignLessonVideoPartResult>> PresignVideoPart(
        Guid courseId,
        Guid lessonId,
        string uploadId,
        [FromBody] PresignLessonVideoPartBody? body,
        CancellationToken cancellationToken)
    {
        if (body == null)
        {
            return BadRequest(new { error = "PartNumber is required.", statusCode = 400 });
        }

        var result = await _mediator.Send(new PresignLessonVideoPartCommand(courseId, lessonId, uploadId, body.PartNumber), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{lessonId:guid}/video/uploads/{uploadId}/complete")]
    [Authorize(Policy = CoursePlatform.Application.Common.Authorization.AuthorizationPolicies.InstructorOrAdmin)]
    public async Task<ActionResult> CompleteVideoUpload(
        Guid courseId,
        Guid lessonId,
        string uploadId,
        [FromBody] CompleteLessonVideoUploadBody? body,
        CancellationToken cancellationToken)
    {
        if (body == null || body.Parts == null || body.Parts.Count == 0)
        {
            return BadRequest(new { error = "Parts are required.", statusCode = 400 });
        }

        await _mediator.Send(new CompleteLessonVideoUploadCommand(courseId, lessonId, uploadId, body.Parts), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{lessonId:guid}/video/uploads/{uploadId}")]
    [Authorize(Policy = CoursePlatform.Application.Common.Authorization.AuthorizationPolicies.InstructorOrAdmin)]
    public async Task<ActionResult> AbortVideoUpload(
        Guid courseId,
        Guid lessonId,
        string uploadId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new AbortLessonVideoUploadCommand(courseId, lessonId, uploadId), cancellationToken);
        return NoContent();
    }

    public sealed record PresignLessonVideoPartBody(int PartNumber);

    public sealed record CompleteLessonVideoUploadBody(List<CoursePlatform.Application.Common.Models.CompletedPart> Parts);

    public sealed record UpdateWatchPositionRequest(int PositionSeconds);
}
