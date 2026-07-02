using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoursePlatform.Application.Features.Courses.Queries.GetCourses;
using CoursePlatform.Application.Features.Courses.Queries.GetCourseDetails;
using CoursePlatform.Application.Features.Courses.Queries.GetCategories;
using CoursePlatform.Application.Features.Courses.Queries.GetTechnologies;
using CoursePlatform.Application.Features.Courses.Commands.CreateCourse;
using CoursePlatform.Application.Features.Courses.Commands.UpdateCourse;
using CoursePlatform.Application.Features.Courses.Commands.ThumbnailUploads;
using CoursePlatform.Application.Features.Courses.Queries.GetInstructorCourses;
using CoursePlatform.Application.Features.Courses.Queries.GetCourseThumbnailUrl;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("api")]
public class CoursesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CoursesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<CoursesVm>> GetCourses(
        [FromQuery] string? searchTerm,
        [FromQuery] CourseLevel? level,
        [FromQuery] CourseStatus? status,
        [FromQuery] string? sortBy,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? language,
        [FromQuery] List<Guid>? categoryIds,
        [FromQuery] List<Guid>? technologyIds,
        [FromQuery] double? minRating,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetCoursesQuery(searchTerm, level, status, sortBy, minPrice, maxPrice, language, categoryIds, technologyIds, minRating, pageNumber, pageSize),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("technologies")]
    [AllowAnonymous]
    public async Task<ActionResult<List<TechnologyDto>>> GetTechnologies(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTechnologiesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("instructor/my")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<ActionResult<List<InstructorCourseDto>>> GetInstructorCourses(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetInstructorCoursesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<CourseDetailsDto>> GetCourseDetails(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCourseDetailsQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<ActionResult<Guid>> CreateCourse(CreateCourseCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<ActionResult> UpdateCourse(Guid id, UpdateCourseCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{courseId:guid}/thumbnail/presign")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<ActionResult<PresignCourseThumbnailUploadResult>> PresignThumbnail(
        Guid courseId,
        [FromBody] PresignCourseThumbnailUploadBody? body,
        CancellationToken cancellationToken)
    {
        if (body == null || string.IsNullOrWhiteSpace(body.ContentType))
        {
            return BadRequest(new { error = "ContentType is required.", statusCode = 400 });
        }

        var result = await _mediator.Send(new PresignCourseThumbnailUploadCommand(courseId, body.ContentType), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{courseId:guid}/thumbnail/confirm")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<ActionResult> ConfirmThumbnail(
        Guid courseId,
        [FromBody] ConfirmCourseThumbnailUploadBody? body,
        CancellationToken cancellationToken)
    {
        if (body == null || string.IsNullOrWhiteSpace(body.ObjectKey))
        {
            return BadRequest(new { error = "ObjectKey is required.", statusCode = 400 });
        }

        await _mediator.Send(new ConfirmCourseThumbnailUploadCommand(courseId, body.ObjectKey), cancellationToken);
        return NoContent();
    }

    [HttpGet("{courseId:guid}/thumbnail")]
    [AllowAnonymous]
    public async Task<ActionResult<CourseThumbnailUrlDto>> GetThumbnailUrl(Guid courseId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCourseThumbnailUrlQuery(courseId), cancellationToken);
        return Ok(result);
    }

    public sealed record PresignCourseThumbnailUploadBody(string ContentType);

    public sealed record ConfirmCourseThumbnailUploadBody(string ObjectKey);
}
