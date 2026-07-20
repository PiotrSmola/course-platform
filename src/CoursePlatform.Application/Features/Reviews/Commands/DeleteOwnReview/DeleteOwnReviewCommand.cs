using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Reviews.Commands.DeleteOwnReview;

public record DeleteOwnReviewCommand(Guid CourseId, Guid ReviewId) : IRequest;

public class DeleteOwnReviewCommandHandler : IRequestHandler<DeleteOwnReviewCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICourseIndexingService _courseIndexing;
    private readonly IAppCache _cache;

    public DeleteOwnReviewCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICourseIndexingService courseIndexing,
        IAppCache cache)
    {
        _context = context;
        _currentUserService = currentUserService;
        _courseIndexing = courseIndexing;
        _cache = cache;
    }

    public async Task Handle(DeleteOwnReviewCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var review = await _context.Reviews
            .FirstOrDefaultAsync(
                r => r.Id == request.ReviewId && r.CourseId == request.CourseId,
                cancellationToken);

        if (review == null)
        {
            throw new NotFoundException($"Review {request.ReviewId} not found.");
        }

        if (review.UserId != _currentUserService.UserId.Value && !_currentUserService.IsAdmin)
        {
            throw new ForbiddenAccessException("You can only delete your own review.");
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync(cancellationToken);
        await _courseIndexing.IndexCourseAsync(request.CourseId, cancellationToken);
        await _cache.InvalidateTagAsync("courses", cancellationToken);
    }
}
