using FluentValidation;
using MediatR;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IIdentityService _identityService;

    public LogoutCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var storedToken = await _identityService.GetRefreshTokenAsync(request.RefreshToken, cancellationToken);

        // Idempotent: logging out with an unknown/expired token is a no-op, not an error.
        if (storedToken is { IsRevoked: false })
        {
            await _identityService.RevokeRefreshTokenAsync(storedToken, cancellationToken);
        }
    }
}
