using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Options;

namespace CoursePlatform.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailQueue _emailQueue;
    private readonly FrontendOptions _frontendOptions;

    public ForgotPasswordCommandHandler(
        IIdentityService identityService,
        IEmailQueue emailQueue,
        IOptions<FrontendOptions> frontendOptions)
    {
        _identityService = identityService;
        _emailQueue = emailQueue;
        _frontendOptions = frontendOptions.Value;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindByEmailAsync(request.Email, cancellationToken);
        if (user == null || string.IsNullOrWhiteSpace(user.Email))
        {
            return;
        }

        try
        {
            var token = await _identityService.GeneratePasswordResetTokenAsync(user, cancellationToken);
            var baseUrl = _frontendOptions.BaseUrl.TrimEnd('/');
            var resetUrl =
                $"{baseUrl}/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";
            var (subject, html) = EmailTemplates.PasswordReset(user.FirstName, resetUrl);
            _emailQueue.Enqueue(new EmailMessage(user.Email, subject, html));
        }
        catch
        {
        }
    }
}
