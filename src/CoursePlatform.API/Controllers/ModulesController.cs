using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoursePlatform.Application.Features.Modules.Commands.CreateModule;
using CoursePlatform.Application.Features.Modules.Commands.UpdateModule;
using CoursePlatform.Application.Features.Modules.Commands.DeleteModule;
using CoursePlatform.Application.Features.Lessons.Commands.CreateLesson;
using CoursePlatform.Application.Features.Lessons.Commands.UpdateLesson;
using CoursePlatform.Application.Features.Lessons.Commands.DeleteLesson;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/courses/{courseId:guid}/modules")]
[Authorize(Policy = CoursePlatform.Application.Common.Authorization.AuthorizationPolicies.InstructorOrAdmin)]
[EnableRateLimiting("api")]
public class ModulesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ModulesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateModule(Guid courseId, CreateModuleCommand command, CancellationToken cancellationToken)
    {
        if (courseId != command.CourseId) return BadRequest();
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{moduleId:guid}")]
    public async Task<ActionResult> UpdateModule(Guid courseId, Guid moduleId, UpdateModuleCommand command, CancellationToken cancellationToken)
    {
        if (courseId != command.CourseId || moduleId != command.ModuleId) return BadRequest();
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{moduleId:guid}")]
    public async Task<ActionResult> DeleteModule(Guid courseId, Guid moduleId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteModuleCommand(courseId, moduleId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{moduleId:guid}/lessons")]
    public async Task<ActionResult<Guid>> CreateLesson(Guid courseId, Guid moduleId, CreateLessonCommand command, CancellationToken cancellationToken)
    {
        if (courseId != command.CourseId || moduleId != command.ModuleId) return BadRequest();
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{moduleId:guid}/lessons/{lessonId:guid}")]
    public async Task<ActionResult> UpdateLesson(Guid courseId, Guid moduleId, Guid lessonId, UpdateLessonCommand command, CancellationToken cancellationToken)
    {
        if (courseId != command.CourseId || moduleId != command.ModuleId || lessonId != command.LessonId)
            return BadRequest();
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{moduleId:guid}/lessons/{lessonId:guid}")]
    public async Task<ActionResult> DeleteLesson(Guid courseId, Guid moduleId, Guid lessonId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteLessonCommand(courseId, moduleId, lessonId), cancellationToken);
        return NoContent();
    }
}
