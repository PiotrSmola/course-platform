using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Wishlists.Commands.AddToWishlist;

public record AddToWishlistCommand(Guid CourseId) : IRequest<Guid>;

public class AddToWishlistCommandValidator : AbstractValidator<AddToWishlistCommand>
{
    public AddToWishlistCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
    }
}

public class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AddToWishlistCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);

        if (course == null)
        {
            throw new NotFoundException($"Course {request.CourseId} not found.");
        }

        if (course.Status != CourseStatus.Published)
        {
            throw new ForbiddenAccessException("Only published courses can be added to the wishlist.");
        }

        var userId = _currentUserService.UserId.Value;
        var existing = await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.CourseId == request.CourseId, cancellationToken);

        if (existing != null)
        {
            return existing.Id;
        }

        var item = new WishlistItem
        {
            UserId = userId,
            CourseId = request.CourseId
        };

        _context.WishlistItems.Add(item);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var alreadyExists = await _context.WishlistItems
                .AnyAsync(w => w.UserId == userId && w.CourseId == request.CourseId, cancellationToken);

            if (!alreadyExists)
            {
                throw;
            }

            throw new ValidationException(new[] { new ValidationFailure("CourseId", "Kurs jest już na liście życzeń.") });
        }

        return item.Id;
    }
}
