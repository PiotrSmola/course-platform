using MediatR;
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
    private readonly IIdentityService _identityService;

    public DeleteAccountCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IIdentityService identityService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _identityService = identityService;
    }

    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
            throw new ForbiddenAccessException();

        var user = await _identityService.FindByIdAsync(_currentUserService.UserId.Value, cancellationToken);
        if (user == null)
            throw new NotFoundException("User", _currentUserService.UserId.Value.ToString());

        var hasCourses = await _context.Courses.AnyAsync(c => c.InstructorId == user.Id, cancellationToken);
        if (hasCourses)
        {
            throw new ValidationException(new[] { new ValidationFailure("", "Cannot delete account with active courses. Remove or transfer them first.") });
        }

        var hasPayments = await _context.Payments.AnyAsync(p => p.UserId == user.Id, cancellationToken);
        if (hasPayments)
        {
            throw new ValidationException(new[] { new ValidationFailure("", "Cannot delete account with payment history.") });
        }

        var hasGiftPurchases = await _context.GiftPurchases.AnyAsync(
            gift => gift.BuyerUserId == user.Id || gift.RedeemedByUserId == user.Id,
            cancellationToken);
        if (hasGiftPurchases)
        {
            throw new ValidationException(new[] { new ValidationFailure("", "Cannot delete account with gift purchase history.") });
        }
        var result = await _identityService.DeleteUserAsync(user, cancellationToken);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors));
        }
    }
}
