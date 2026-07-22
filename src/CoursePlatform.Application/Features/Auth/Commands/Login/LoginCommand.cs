using MediatR;
using FluentValidation;
using FluentValidation.Results;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(IIdentityService identityService, IJwtTokenGenerator jwtTokenGenerator)
    {
        _identityService = identityService;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            throw new ValidationException(new[] { new ValidationFailure("Email", "Invalid email or password.") });
        }

        var result = await _identityService.CheckPasswordAsync(user, request.Password, cancellationToken);
        if (result == AuthPasswordVerificationResult.NotAllowed)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("Email", "Potwierdź adres email przed zalogowaniem.")
            });
        }

        if (result == AuthPasswordVerificationResult.LockedOut)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("Email", "Konto jest tymczasowo zablokowane. Spróbuj ponownie później.")
            });
        }

        if (result != AuthPasswordVerificationResult.Success)
        {
            throw new ValidationException(new[] { new ValidationFailure("Password", "Invalid email or password.") });
        }

        var roles = await _identityService.GetRolesAsync(user, cancellationToken);
        var token = _jwtTokenGenerator.GenerateToken(user, roles);
        var refreshToken = await _identityService.CreateRefreshTokenAsync(user.Id, cancellationToken);

        return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, refreshToken.RawToken, roles.ToList());
    }
}
