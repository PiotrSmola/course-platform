using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<ApplicationUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApplicationUser>> GetUsersAsync(CancellationToken cancellationToken = default);

    Task<IdentityOperationResult> CreateUserAsync(ApplicationUser user, string password, CancellationToken cancellationToken = default);
    Task<IdentityOperationResult> AddToRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default);
    Task<IdentityOperationResult> AddToRoleAsync(ApplicationUser user, string role, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task<IdentityOperationResult> DeleteUserAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task<bool> IsInRoleAsync(ApplicationUser user, string role, CancellationToken cancellationToken = default);
    Task<bool> IsLockedOutAsync(ApplicationUser user, CancellationToken cancellationToken = default);

    Task<AuthPasswordVerificationResult> CheckPasswordAsync(ApplicationUser user, string password, CancellationToken cancellationToken = default);

    Task<IssuedRefreshToken> CreateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task RevokeAllRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}

public sealed record IssuedRefreshToken(RefreshToken Entity, string RawToken);

public sealed record IdentityOperationResult(bool Succeeded, IReadOnlyList<string> Errors)
{
    public static IdentityOperationResult Success() => new(true, Array.Empty<string>());
    public static IdentityOperationResult Failure(IEnumerable<string> errors) => new(false, errors.ToList());
}

public enum AuthPasswordVerificationResult
{
    Success,
    Failed,
    LockedOut,
    NotAllowed
}
