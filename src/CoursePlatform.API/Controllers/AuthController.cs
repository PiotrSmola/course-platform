using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoursePlatform.Application.Features.Auth;
using CoursePlatform.Application.Features.Auth.Commands.Login;
using CoursePlatform.Application.Features.Auth.Commands.Register;
using CoursePlatform.Application.Features.Auth.Queries.GetCurrentUser;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("api")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponse>> Login(LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<CurrentUserDto?>> GetCurrentUser()
    {
        var result = await _mediator.Send(new GetCurrentUserQuery());
        if (result == null) return Unauthorized();
        return Ok(result);
    }
}