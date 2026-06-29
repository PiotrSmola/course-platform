using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoursePlatform.Application.Features.Admin.Commands.AssignUserRole;
using CoursePlatform.Application.Features.Admin.Commands.DeleteReview;
using CoursePlatform.Application.Features.Admin.Commands.UpdateCourseStatus;
using CoursePlatform.Application.Features.Admin.Queries.GetUsers;
using CoursePlatform.Application.Features.Courses.Queries.GetCourses;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting("api")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserDto>>> GetUsers()
    {
        var result = await _mediator.Send(new GetUsersQuery());
        return Ok(result);
    }

    [HttpPost("users/{userId:guid}/roles")]
    public async Task<ActionResult> AssignRole(Guid userId, AssignUserRoleCommand command)
    {
        if (userId != command.UserId) return BadRequest();
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpGet("courses")]
    public async Task<ActionResult<CoursesVm>> GetCourses(
        [FromQuery] string? searchTerm,
        [FromQuery] CourseLevel? level,
        [FromQuery] CourseStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetCoursesQuery(
            searchTerm, level, status, null, null, null, null, null, null, null, pageNumber, pageSize));
        return Ok(result);
    }

    [HttpPut("courses/{courseId:guid}/status")]
    public async Task<ActionResult> UpdateCourseStatus(Guid courseId, UpdateCourseStatusCommand command)
    {
        if (courseId != command.CourseId) return BadRequest();
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("reviews/{reviewId:guid}")]
    public async Task<ActionResult> DeleteReview(Guid reviewId)
    {
        await _mediator.Send(new DeleteReviewCommand(reviewId));
        return NoContent();
    }
}
