using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Features.Admin.Queries.GetUsers;

public record GetUsersQuery() : IRequest<List<AdminUserDto>>;

public record AdminUserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    List<string> Roles,
    bool IsLockedOut);

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<AdminUserDto>>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;

    public GetUsersQueryHandler(
        IIdentityService identityService,
        ICurrentUserService currentUserService)
    {
        _identityService = identityService;
        _currentUserService = currentUserService;
    }

    public async Task<List<AdminUserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdmin)
        {
            throw new ForbiddenAccessException("Admin access required.");
        }

        var users = await _identityService.GetUsersAsync(cancellationToken);

        var result = new List<AdminUserDto>();

        foreach (var user in users)
        {
            var roles = await _identityService.GetRolesAsync(user, cancellationToken);
            var isLockedOut = await _identityService.IsLockedOutAsync(user, cancellationToken);
            result.Add(new AdminUserDto(
                user.Id,
                user.Email ?? string.Empty,
                user.FirstName,
                user.LastName,
                roles.ToList(),
                isLockedOut));
        }

        return result;
    }
}
