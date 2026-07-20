using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoursePlatform.Application.Common.Authorization;
using CoursePlatform.Application.Features.Auth;
using CoursePlatform.Application.Features.Auth.Commands.ConfirmEmail;
using CoursePlatform.Application.Features.Auth.Commands.ForgotPassword;
using CoursePlatform.Application.Features.Auth.Commands.Login;
using CoursePlatform.Application.Features.Auth.Commands.Logout;
using CoursePlatform.Application.Features.Auth.Commands.RefreshToken;
using CoursePlatform.Application.Features.Auth.Commands.Register;
using CoursePlatform.Application.Features.Auth.Commands.ResetPassword;
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
    public async Task<ActionResult<AuthResponse>> Register(RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponse>> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("confirm-email")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(LogoutCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<CurrentUserDto?>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCurrentUserQuery(), cancellationToken);
        if (result == null) return Unauthorized();
        return Ok(result);
    }
}
