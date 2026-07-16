using MediatR;
using FluentValidation;
using FluentValidation.Results;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : IRequest<AuthResponse>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEmailQueue _emailQueue;

    public RegisterCommandHandler(
        IIdentityService identityService,
        IJwtTokenGenerator jwtTokenGenerator,
        IEmailQueue emailQueue)
    {
        _identityService = identityService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _emailQueue = emailQueue;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var createResult = await _identityService.CreateUserAsync(user, request.Password, cancellationToken);
        if (!createResult.Succeeded)
        {
            throw new ValidationException(createResult.Errors.Select(e => new ValidationFailure(string.Empty, e)));
        }

        var roleResult = await _identityService.AddToRoleAsync(user, "Student", cancellationToken);
        if (!roleResult.Succeeded)
        {
            throw new ValidationException(roleResult.Errors.Select(e => new ValidationFailure(string.Empty, e)));
        }

        var (subject, html) = EmailTemplates.Welcome(user.FirstName);
        _emailQueue.Enqueue(new EmailMessage(user.Email!, subject, html));

        var roles = await _identityService.GetRolesAsync(user, cancellationToken);
        var token = _jwtTokenGenerator.GenerateToken(user, roles);
        var refreshToken = await _identityService.CreateRefreshTokenAsync(user.Id, cancellationToken);

        return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, refreshToken.Token, roles.ToList());
    }
}
