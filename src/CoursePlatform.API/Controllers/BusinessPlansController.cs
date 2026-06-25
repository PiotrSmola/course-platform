using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoursePlatform.Application.Features.BusinessPlans.Queries.GetBusinessPlans;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/business-plans")]
public class BusinessPlansController : ControllerBase
{
    private readonly IMediator _mediator;

    public BusinessPlansController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<BusinessPlansVm>> GetBusinessPlans()
    {
        var result = await _mediator.Send(new GetBusinessPlansQuery());
        return Ok(result);
    }
}