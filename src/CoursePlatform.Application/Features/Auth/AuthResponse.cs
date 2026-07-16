namespace CoursePlatform.Application.Features.Auth;

public record AuthResponse(Guid Id, string? Email, string FirstName, string LastName, string Token, string RefreshToken, List<string> Roles);
