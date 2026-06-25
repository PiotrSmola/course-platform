using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoursePlatform.Application.Features.LearningPaths.Queries.GetLearningPaths;
using CoursePlatform.Application.Features.LearningPaths.Queries.GetLearningPathBySlug;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/learning-paths")]
public class LearningPathsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LearningPathsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<LearningPathsVm>> GetLearningPaths()
    {
        var result = await _mediator.Send(new GetLearningPathsQuery());
        return Ok(result);
    }

    [HttpGet("{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<LearningPathDetailsDto>> GetLearningPathBySlug(string slug)
    {
        var result = await _mediator.Send(new GetLearningPathBySlugQuery(slug));
        return Ok(result);
    }
}