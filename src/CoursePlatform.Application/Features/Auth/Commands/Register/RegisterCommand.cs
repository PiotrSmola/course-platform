using MediatR;
using Microsoft.AspNetCore.Identity;
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
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEmailQueue _emailQueue;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator jwtTokenGenerator,
        IEmailQueue emailQueue)
    {
        _userManager = userManager;
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

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(e => new ValidationFailure(e.Code, e.Description)));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "Student");
        if (!roleResult.Succeeded)
        {
            throw new ValidationException(roleResult.Errors.Select(e => new ValidationFailure(e.Code, e.Description)));
        }

        var (subject, html) = EmailTemplates.Welcome(user.FirstName);
        _emailQueue.Enqueue(new EmailMessage(user.Email!, subject, html));

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

        return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, roles.ToList());
    }
}
