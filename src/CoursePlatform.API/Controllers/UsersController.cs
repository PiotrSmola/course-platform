using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoursePlatform.Application.Features.Users.Queries.GetUserProfile;
using CoursePlatform.Application.Features.Users.Commands.UpdateProfile;
using CoursePlatform.Application.Features.Users.Commands.DeleteAccount;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
[EnableRateLimiting("api")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var result = await _mediator.Send(new GetUserProfileQuery());
        return Ok(result);
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserProfileSummaryDto>> UpdateProfile(UpdateProfileCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("me")]
    public async Task<IActionResult> DeleteAccount()
    {
        await _mediator.Send(new DeleteAccountCommand());
        return NoContent();
    }
}
