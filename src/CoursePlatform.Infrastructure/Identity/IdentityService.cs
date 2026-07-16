using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IApplicationDbContext _context;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<ApplicationUser?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByIdAsync(userId.ToString());
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync(cancellationToken);
    }

    public async Task<IdentityOperationResult> CreateUserAsync(ApplicationUser user, string password, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.CreateAsync(user, password);
        return MapResult(result);
    }

    public async Task<IdentityOperationResult> AddToRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return IdentityOperationResult.Failure(new[] { "User not found." });
        }

        return await AddToRoleAsync(user, role, cancellationToken);
    }

    public async Task<IdentityOperationResult> AddToRoleAsync(ApplicationUser user, string role, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.AddToRoleAsync(user, role);
        return MapResult(result);
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        return (await _userManager.GetRolesAsync(user)).ToList();
    }

    public async Task<IdentityOperationResult> DeleteUserAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.DeleteAsync(user);
        return MapResult(result);
    }

    public async Task<bool> IsInRoleAsync(ApplicationUser user, string role, CancellationToken cancellationToken = default)
    {
        return await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> IsLockedOutAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        return await _userManager.IsLockedOutAsync(user);
    }

    public async Task<AuthPasswordVerificationResult> CheckPasswordAsync(ApplicationUser user, string password, CancellationToken cancellationToken = default)
    {
        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

        if (result.Succeeded) return AuthPasswordVerificationResult.Success;
        if (result.IsLockedOut) return AuthPasswordVerificationResult.LockedOut;
        if (result.IsNotAllowed) return AuthPasswordVerificationResult.NotAllowed;
        return AuthPasswordVerificationResult.Failed;
    }

    public async Task<RefreshToken> CreateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);
        return refreshToken;
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task RevokeRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        token.IsRevoked = true;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static IdentityOperationResult MapResult(IdentityResult result)
    {
        return result.Succeeded
            ? IdentityOperationResult.Success()
            : IdentityOperationResult.Failure(result.Errors.Select(e => e.Description));
    }
}
