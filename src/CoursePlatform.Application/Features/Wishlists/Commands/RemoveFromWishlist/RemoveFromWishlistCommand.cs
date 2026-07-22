using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Exceptions;

namespace CoursePlatform.Application.Features.Wishlists.Commands.RemoveFromWishlist;

public record RemoveFromWishlistCommand(Guid CourseId) : IRequest;

public class RemoveFromWishlistCommandValidator : AbstractValidator<RemoveFromWishlistCommand>
{
    public RemoveFromWishlistCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
    }
}

public class RemoveFromWishlistCommandHandler : IRequestHandler<RemoveFromWishlistCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RemoveFromWishlistCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var item = await _context.WishlistItems
            .FirstOrDefaultAsync(
                w => w.UserId == _currentUserService.UserId.Value && w.CourseId == request.CourseId,
                cancellationToken);

        if (item == null)
        {
            throw new NotFoundException($"Wishlist item for course {request.CourseId} not found.");
        }

        _context.WishlistItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
