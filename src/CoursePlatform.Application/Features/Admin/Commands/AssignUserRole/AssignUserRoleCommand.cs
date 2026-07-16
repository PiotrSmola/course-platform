using FluentValidation;
using FluentValidation.Results;
using MediatR;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Features.Admin.Commands.AssignUserRole;

public record AssignUserRoleCommand(Guid UserId, string Role) : IRequest;

public class AssignUserRoleCommandValidator : AbstractValidator<AssignUserRoleCommand>
{
    private static readonly string[] AllowedRoles = ["Student", "Instructor", "Admin"];

    public AssignUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(role => AllowedRoles.Contains(role))
            .WithMessage("Role must be Student, Instructor, or Admin.");
    }
}

public class AssignUserRoleCommandHandler : IRequestHandler<AssignUserRoleCommand>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLog;

    public AssignUserRoleCommandHandler(IIdentityService identityService, ICurrentUserService currentUserService, IAuditLogService auditLog)
    {
        _identityService = identityService;
        _currentUserService = currentUserService;
        _auditLog = auditLog;
    }

    public async Task Handle(AssignUserRoleCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdmin)
        {
            throw new ForbiddenAccessException("Admin access required.");
        }

        var user = await _identityService.FindByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException($"User {request.UserId} not found.");
        }

        if (await _identityService.IsInRoleAsync(user, request.Role, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(request.Role), $"User already has role {request.Role}.")
            });
        }

        var result = await _identityService.AddToRoleAsync(user, request.Role, cancellationToken);
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(e => new ValidationFailure(string.Empty, e)));
        }

        await _auditLog.LogAsync(
            "AssignUserRole",
            "User",
            user.Id.ToString(),
            $"Role '{request.Role}' assigned to user '{user.Email}' by admin.",
            cancellationToken);
    }
}
