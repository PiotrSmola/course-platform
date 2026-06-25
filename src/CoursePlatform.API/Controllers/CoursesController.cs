using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Features.Courses.Queries.GetCourses;
using CoursePlatform.Application.Features.Courses.Queries.GetCourseDetails;
using CoursePlatform.Application.Features.Courses.Commands.CreateCourse;
using CoursePlatform.Application.Features.Courses.Commands.UpdateCourse;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;

    public CoursesController(IMediator mediator, IApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
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
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetCoursesQuery(searchTerm, level, status, sortBy, minPrice, maxPrice, language, categoryIds, technologyIds, minRating, pageNumber, pageSize));
        return Ok(result);
    }

    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<ActionResult> GetCategories()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name, c.Slug })
            .ToListAsync();
        return Ok(categories);
    }

    [HttpGet("technologies")]
    [AllowAnonymous]
    public async Task<ActionResult> GetTechnologies()
    {
        var technologies = await _context.Technologies
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new { t.Id, t.Name, t.Slug })
            .ToListAsync();
        return Ok(technologies);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<CourseDetailsDto>> GetCourseDetails(Guid id)
    {
        var result = await _mediator.Send(new GetCourseDetailsQuery(id));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<ActionResult<Guid>> CreateCourse(CreateCourseCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<ActionResult> UpdateCourse(Guid id, UpdateCourseCommand command)
    {
        if (id != command.Id) return BadRequest();
        await _mediator.Send(command);
        return NoContent();
    }
}
