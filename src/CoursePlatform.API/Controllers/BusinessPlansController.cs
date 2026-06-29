using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoursePlatform.Application.Features.BusinessPlans.Queries.GetBusinessPlans;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/business-plans")]
[EnableRateLimiting("api")]
public class BusinessPlansController : ControllerBase
{
    private readonly IMediator _mediator;

    public BusinessPlansController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<BusinessPlansVm>> GetBusinessPlans(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBusinessPlansQuery(), cancellationToken);
        return Ok(result);
    }
}
