using MediatR;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Auth.Queries.GetCurrentUser;

public record GetCurrentUserQuery : IRequest<CurrentUserDto?>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserDto?>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentUserQueryHandler(IIdentityService identityService, ICurrentUserService currentUserService)
    {
        _identityService = identityService;
        _currentUserService = currentUserService;
    }

    public async Task<CurrentUserDto?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null) return null;

        var user = await _identityService.FindByIdAsync(_currentUserService.UserId.Value, cancellationToken);
        if (user == null) return null;

        var roles = await _identityService.GetRolesAsync(user, cancellationToken);
        return new CurrentUserDto(user.Id, user.Email, user.FirstName, user.LastName, roles.ToList());
    }
}

public record CurrentUserDto(Guid Id, string? Email, string FirstName, string LastName, List<string> Roles);
