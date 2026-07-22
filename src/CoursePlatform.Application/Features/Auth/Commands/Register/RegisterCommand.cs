using MediatR;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Options;

namespace CoursePlatform.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : IRequest<RegisterResponse>;

public record RegisterResponse(
    Guid Id,
    string? Email,
    string FirstName,
    string LastName,
    string Message);

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailQueue _emailQueue;
    private readonly FrontendOptions _frontendOptions;

    public RegisterCommandHandler(
        IIdentityService identityService,
        IEmailQueue emailQueue,
        IOptions<FrontendOptions> frontendOptions)
    {
        _identityService = identityService;
        _emailQueue = emailQueue;
        _frontendOptions = frontendOptions.Value;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
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

        try
        {
            var (welcomeSubject, welcomeHtml) = EmailTemplates.Welcome(user.FirstName);
            _emailQueue.Enqueue(new EmailMessage(user.Email!, welcomeSubject, welcomeHtml));

            var confirmToken = await _identityService.GenerateEmailConfirmationTokenAsync(user, cancellationToken);
            var baseUrl = _frontendOptions.BaseUrl.TrimEnd('/');
            var confirmUrl =
                $"{baseUrl}/confirm-email?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(confirmToken)}";
            var (confirmSubject, confirmHtml) = EmailTemplates.ConfirmEmail(user.FirstName, confirmUrl);
            _emailQueue.Enqueue(new EmailMessage(user.Email!, confirmSubject, confirmHtml));
        }
        catch
        {
        }

        return new RegisterResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            "Konto utworzone. Potwierdź adres email, zanim się zalogujesz.");
    }
}
