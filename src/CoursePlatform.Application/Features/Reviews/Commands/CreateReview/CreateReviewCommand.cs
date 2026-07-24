using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Features.Reviews.Commands.CreateReview;

public record CreateReviewCommand(Guid CourseId, int Rating, string Comment) : IRequest<Guid>;

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHtmlSanitizer _htmlSanitizer;
    private readonly ICourseIndexingService _courseIndexing;
    private readonly IAppCache _cache;

    public CreateReviewCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IHtmlSanitizer htmlSanitizer, ICourseIndexingService courseIndexing, IAppCache cache)
    {
        _context = context;
        _currentUserService = currentUserService;
        _htmlSanitizer = htmlSanitizer;
        _courseIndexing = courseIndexing;
        _cache = cache;
    }

    public async Task<Guid> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;

        var isEnrolled = await _context.Enrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == request.CourseId, cancellationToken);

        var hasSubscriptionAccess = await CourseAccessHelper.HasActiveSubscriptionAsync(
            _context, userId, cancellationToken);

        if (!isEnrolled && !hasSubscriptionAccess)
        {
            throw new ForbiddenAccessException("You must be enrolled in the course or have All-access to leave a review.");
        }

        var existing = await _context.Reviews
            .FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == request.CourseId, cancellationToken);

        if (existing != null)
        {
            throw new ValidationException(new[] { new ValidationFailure("Comment", "Już dodałeś opinię do tego kursu.") });
        }

        var review = new Review
        {
            UserId = userId,
            CourseId = request.CourseId,
            Rating = request.Rating,
            Comment = _htmlSanitizer.Sanitize(request.Comment)
        };

        _context.Reviews.Add(review);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.CourseId == request.CourseId, cancellationToken);

            if (!alreadyReviewed)
            {
                throw;
            }

            throw new ValidationException(new[] { new ValidationFailure("Comment", "Już dodałeś opinię do tego kursu.") });
        }

        await _courseIndexing.IndexCourseAsync(request.CourseId, cancellationToken);
        await _cache.InvalidateTagAsync("courses", cancellationToken);
        return review.Id;
    }
}
