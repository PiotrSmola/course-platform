using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoursePlatform.Application.Features.Courses.Queries.GetCourses;
using CoursePlatform.Application.Features.Courses.Queries.GetCourseDetails;
using CoursePlatform.Application.Features.Courses.Commands.CreateCourse;
using CoursePlatform.Application.Features.Courses.Commands.UpdateCourse;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetCoursesQuery(searchTerm, level, status, pageNumber, pageSize));
        return Ok(result);
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
