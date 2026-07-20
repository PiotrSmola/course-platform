using FluentValidation;
using FluentValidation.Results;
using MediatR;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Validation;

namespace CoursePlatform.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).ValidPassword();
    }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IIdentityService _identityService;

    public ResetPasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(string.Empty, "Nieprawidłowy token lub adres email.")
            });
        }

        var result = await _identityService.ResetPasswordAsync(user, request.Token, request.NewPassword, cancellationToken);
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(e => new ValidationFailure(string.Empty, e)));
        }

        await _identityService.RevokeAllRefreshTokensAsync(user.Id, cancellationToken);
    }
}
