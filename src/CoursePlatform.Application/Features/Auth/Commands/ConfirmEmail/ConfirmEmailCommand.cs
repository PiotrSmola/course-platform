using FluentValidation;
using FluentValidation.Results;
using MediatR;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Auth.Commands.ConfirmEmail;

public record ConfirmEmailCommand(string Email, string Token) : IRequest;

public class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
    }
}

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand>
{
    private readonly IIdentityService _identityService;

    public ConfirmEmailCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(string.Empty, "Nieprawidłowy token lub adres email.")
            });
        }

        var result = await _identityService.ConfirmEmailAsync(user, request.Token, cancellationToken);
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(e => new ValidationFailure(string.Empty, e)));
        }
    }
}
