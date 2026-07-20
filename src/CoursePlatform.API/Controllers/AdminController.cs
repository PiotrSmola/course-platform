using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoursePlatform.Application.Common.Authorization;
using CoursePlatform.Application.Features.Admin.Commands.AssignUserRole;
using CoursePlatform.Application.Features.Admin.Commands.DeleteReview;
using CoursePlatform.Application.Features.Admin.Commands.StartReindex;
using CoursePlatform.Application.Features.Admin.Commands.UpdateCourseStatus;
using CoursePlatform.Application.Features.Admin.Queries.GetReindexJob;
using CoursePlatform.Application.Features.Admin.Queries.GetSearchStats;
using CoursePlatform.Application.Features.Admin.Queries.GetAuditLogs;
using CoursePlatform.Application.Features.Admin.Queries.GetReviews;
using CoursePlatform.Application.Features.Admin.Queries.GetUsers;
using CoursePlatform.Application.Features.Admin.Search;
using CoursePlatform.Application.Features.Courses.Queries.GetCourses;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[EnableRateLimiting("api")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserDto>>> GetUsers(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUsersQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("users/{userId:guid}/roles")]
    public async Task<ActionResult> AssignRole(Guid userId, AssignUserRoleCommand command, CancellationToken cancellationToken)
    {
        if (userId != command.UserId) return BadRequest();
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("courses")]
    public async Task<ActionResult<CoursesVm>> GetCourses(
        [FromQuery] string? searchTerm,
        [FromQuery] CourseLevel? level,
        [FromQuery] CourseStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetCoursesQuery(
            searchTerm, level, status, null, null, null, null, null, null, null, pageNumber, pageSize),
            cancellationToken);
        return Ok(result);
    }

    [HttpPut("courses/{courseId:guid}/status")]
    public async Task<ActionResult> UpdateCourseStatus(Guid courseId, UpdateCourseStatusCommand command, CancellationToken cancellationToken)
    {
        if (courseId != command.CourseId) return BadRequest();
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("reviews")]
    public async Task<ActionResult<List<AdminReviewDto>>> GetReviews(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAdminReviewsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("audit-logs")]
    public async Task<ActionResult<PagedAuditLogsDto>> GetAuditLogs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAuditLogsQuery(pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("reviews/{reviewId:guid}")]
    public async Task<ActionResult> DeleteReview(Guid reviewId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteReviewCommand(reviewId), cancellationToken);
        return NoContent();
    }

    [HttpPost("search/reindex")]
    public async Task<ActionResult<StartReindexResult>> StartReindex(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new StartReindexCommand(), cancellationToken);
        return Accepted(result);
    }

    [HttpGet("search/reindex")]
    public async Task<ActionResult<ReindexJobState?>> GetReindexJob(CancellationToken cancellationToken)
    {
        var state = await _mediator.Send(new GetReindexJobQuery(), cancellationToken);
        return Ok(state);
    }

    [HttpGet("search/stats")]
    public async Task<ActionResult<CourseIndexStats>> GetSearchStats(CancellationToken cancellationToken)
    {
        var stats = await _mediator.Send(new GetSearchStatsQuery(), cancellationToken);
        return Ok(stats);
    }
}
