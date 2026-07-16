using FluentValidation;
using FluentValidation.Results;
using MediatR;
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

    public RefreshTokenCommandHandler(IIdentityService identityService, IJwtTokenGenerator jwtTokenGenerator)
    {
        _identityService = identityService;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var storedToken = await _identityService.GetRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (storedToken == null || !storedToken.IsActive)
        {
            throw new FluentValidation.ValidationException(new[]
            {
                new ValidationFailure(nameof(request.RefreshToken), "Invalid refresh token.")
            });
        }

        await _identityService.RevokeRefreshTokenAsync(storedToken, cancellationToken);

        var user = storedToken.User;
        var roles = await _identityService.GetRolesAsync(user, cancellationToken);
        var token = _jwtTokenGenerator.GenerateToken(user, roles);
        var newRefreshToken = await _identityService.CreateRefreshTokenAsync(user.Id, cancellationToken);

        return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, newRefreshToken.Token, roles.ToList());
    }
}
