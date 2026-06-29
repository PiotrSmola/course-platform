using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.UnitTests.Common;

public static class TestUserManagerFactory
{
    public static UserManager<ApplicationUser> Create()
    {
        var store = new MockUserStore();
        return new UserManager<ApplicationUser>(
            store,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            new LoggerFactory().CreateLogger<UserManager<ApplicationUser>>());
    }
}

internal class MockUserStore : IUserStore<ApplicationUser>, IUserRoleStore<ApplicationUser>
{
    public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default) =>
        Task.FromResult(IdentityResult.Success);

    public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken = default) =>
        Task.FromResult(IdentityResult.Success);

    public Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken = default) =>
        Task.FromResult<ApplicationUser?>(null);

    public Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default) =>
        Task.FromResult<ApplicationUser?>(null);

    public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken = default) =>
        Task.FromResult<string?>(user.UserName);

    public Task<string?> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken = default) =>
        Task.FromResult<string?>(user.Id.ToString());

    public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken = default) =>
        Task.FromResult<string?>(user.Email);

    public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default) =>
        Task.FromResult(IdentityResult.Success);

    public void Dispose() { }

    public Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken = default) =>
        Task.FromResult<IList<string>>(new List<string>());

    public Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken = default) =>
        Task.FromResult(false);

    public Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default) =>
        Task.FromResult<IList<ApplicationUser>>(new List<ApplicationUser>());
}
