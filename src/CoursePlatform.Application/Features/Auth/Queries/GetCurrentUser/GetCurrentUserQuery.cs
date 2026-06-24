using MediatR;
using Microsoft.AspNetCore.Identity;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Auth.Queries.GetCurrentUser;

public record GetCurrentUserQuery : IRequest<CurrentUserDto?>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserDto?>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentUserQueryHandler(UserManager<ApplicationUser> userManager, ICurrentUserService currentUserService)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
    }

    public async Task<CurrentUserDto?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null) return null;

        var user = await _userManager.FindByIdAsync(_currentUserService.UserId.Value.ToString());
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new CurrentUserDto(user.Id, user.Email, user.FirstName, user.LastName, roles.ToList());
    }
}

public record CurrentUserDto(Guid Id, string? Email, string FirstName, string LastName, List<string> Roles);
