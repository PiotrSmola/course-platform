using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Exceptions;
using FluentValidation;
using FluentValidation.Results;

namespace CoursePlatform.Application.Features.Users.Commands.DeleteAccount;

public record DeleteAccountCommand : IRequest;

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<Domain.Entities.ApplicationUser> _userManager;

    public DeleteAccountCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        UserManager<Domain.Entities.ApplicationUser> userManager)
    {
        _context = context;
        _currentUserService = currentUserService;
        _userManager = userManager;
    }

    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
            throw new ForbiddenAccessException();

        var user = await _userManager.FindByIdAsync(_currentUserService.UserId.Value.ToString());
        if (user == null)
            throw new NotFoundException("User", _currentUserService.UserId.Value.ToString());

        var hasCourses = await _context.Courses.AnyAsync(c => c.InstructorId == user.Id, cancellationToken);
        if (hasCourses)
        {
            throw new ValidationException(new[] { new ValidationFailure("", "Cannot delete account with active courses. Remove or transfer them first.") });
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }
}
