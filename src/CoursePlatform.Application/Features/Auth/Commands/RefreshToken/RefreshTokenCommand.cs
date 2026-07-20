using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IIdentityService identityService,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _identityService = identityService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var storedToken = await _identityService.GetRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (storedToken == null)
        {
            throw InvalidToken();
        }

        // Reuse of an already-revoked token signals theft: revoke the whole family and reject.
        if (storedToken.IsRevoked)
        {
            _logger.LogWarning("Refresh token reuse detected for user {UserId}. Revoking all active tokens.", storedToken.UserId);
            await _identityService.RevokeAllRefreshTokensAsync(storedToken.UserId, cancellationToken);
            throw InvalidToken();
        }

        if (!storedToken.IsActive)
        {
            throw InvalidToken();
        }

        await _identityService.RevokeRefreshTokenAsync(storedToken, cancellationToken);

        var user = storedToken.User;
        var roles = await _identityService.GetRolesAsync(user, cancellationToken);
        var token = _jwtTokenGenerator.GenerateToken(user, roles);
        var newRefreshToken = await _identityService.CreateRefreshTokenAsync(user.Id, cancellationToken);

        return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, newRefreshToken.RawToken, roles.ToList());
    }

    private static FluentValidation.ValidationException InvalidToken() =>
        new(new[] { new ValidationFailure("RefreshToken", "Invalid refresh token.") });
}
