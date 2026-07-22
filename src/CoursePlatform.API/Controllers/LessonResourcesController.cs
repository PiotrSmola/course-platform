using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoursePlatform.Application.Common.Authorization;
using CoursePlatform.Application.Features.LessonResources;
using CoursePlatform.Application.Features.LessonResources.Commands.ConfirmLessonResourceUpload;
using CoursePlatform.Application.Features.LessonResources.Commands.DeleteLessonResource;
using CoursePlatform.Application.Features.LessonResources.Commands.PresignLessonResourceUpload;
using CoursePlatform.Application.Features.LessonResources.Queries.GetLessonResourceDownloadUrl;
using CoursePlatform.Application.Features.LessonResources.Queries.GetLessonResources;
using CoursePlatform.Application.Features.LessonResources.Queries.GetLessonResourcesForInstructor;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/courses/{courseId:guid}/lessons/{lessonId:guid}/resources")]
[Authorize]
[EnableRateLimiting("api")]
public class LessonResourcesController : ControllerBase
{
    private readonly IMediator _mediator;

    public LessonResourcesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LessonResourceDto>>> GetResources(
        Guid courseId,
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLessonResourcesQuery(courseId, lessonId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("manage")]
    [Authorize(Policy = AuthorizationPolicies.InstructorOrAdmin)]
    public async Task<ActionResult<IReadOnlyList<LessonResourceDto>>> GetResourcesForInstructor(
        Guid courseId,
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetLessonResourcesForInstructorQuery(courseId, lessonId),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("presign")]
    [Authorize(Policy = AuthorizationPolicies.InstructorOrAdmin)]
    public async Task<ActionResult<PresignLessonResourceUploadResult>> PresignUpload(
        Guid courseId,
        Guid lessonId,
        [FromBody] PresignLessonResourceUploadBody? body,
        CancellationToken cancellationToken)
    {
        if (body == null || string.IsNullOrWhiteSpace(body.ContentType))
        {
            return BadRequest(new { message = "ContentType is required." });
        }

        var result = await _mediator.Send(
            new PresignLessonResourceUploadCommand(courseId, lessonId, body.ContentType),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("confirm")]
    [Authorize(Policy = AuthorizationPolicies.InstructorOrAdmin)]
    public async Task<ActionResult<Guid>> ConfirmUpload(
        Guid courseId,
        Guid lessonId,
        [FromBody] ConfirmLessonResourceUploadBody? body,
        CancellationToken cancellationToken)
    {
        if (body == null
            || string.IsNullOrWhiteSpace(body.ObjectKey)
            || string.IsNullOrWhiteSpace(body.Title)
            || string.IsNullOrWhiteSpace(body.ContentType))
        {
            return BadRequest(new { message = "ResourceId, ObjectKey, Title and ContentType are required." });
        }

        var result = await _mediator.Send(
            new ConfirmLessonResourceUploadCommand(
                courseId,
                lessonId,
                body.ResourceId,
                body.ObjectKey,
                body.Title,
                body.ContentType,
                body.Order),
            cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{resourceId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.InstructorOrAdmin)]
    public async Task<ActionResult> DeleteResource(
        Guid courseId,
        Guid lessonId,
        Guid resourceId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeleteLessonResourceCommand(courseId, lessonId, resourceId),
            cancellationToken);
        return NoContent();
    }

    [HttpGet("{resourceId:guid}/download")]
    public async Task<ActionResult<LessonResourceDownloadDto>> GetDownloadUrl(
        Guid courseId,
        Guid lessonId,
        Guid resourceId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetLessonResourceDownloadUrlQuery(courseId, lessonId, resourceId),
            cancellationToken);
        return Ok(result);
    }
}

public record PresignLessonResourceUploadBody(string ContentType);

public record ConfirmLessonResourceUploadBody(
    Guid ResourceId,
    string ObjectKey,
    string Title,
    string ContentType,
    int? Order);
