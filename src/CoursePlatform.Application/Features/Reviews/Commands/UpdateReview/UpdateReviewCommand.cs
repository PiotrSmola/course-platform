using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Reviews.Commands.UpdateReview;

public record UpdateReviewCommand(Guid CourseId, Guid ReviewId, int Rating, string Comment) : IRequest;

public class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHtmlSanitizer _htmlSanitizer;
    private readonly ICourseIndexingService _courseIndexing;
    private readonly IAppCache _cache;

    public UpdateReviewCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IHtmlSanitizer htmlSanitizer,
        ICourseIndexingService courseIndexing,
        IAppCache cache)
    {
        _context = context;
        _currentUserService = currentUserService;
        _htmlSanitizer = htmlSanitizer;
        _courseIndexing = courseIndexing;
        _cache = cache;
    }

    public async Task Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
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
            throw new ForbiddenAccessException("You can only edit your own review.");
        }

        review.Rating = request.Rating;
        review.Comment = _htmlSanitizer.Sanitize(request.Comment);
        review.MarkUpdated();

        await _context.SaveChangesAsync(cancellationToken);
        await _courseIndexing.IndexCourseAsync(request.CourseId, cancellationToken);
        await _cache.InvalidateTagAsync("courses", cancellationToken);
    }
}
