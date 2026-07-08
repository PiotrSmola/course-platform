using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

    public CreateReviewCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IHtmlSanitizer htmlSanitizer, ICourseIndexingService courseIndexing)
    {
        _context = context;
        _currentUserService = currentUserService;
        _htmlSanitizer = htmlSanitizer;
        _courseIndexing = courseIndexing;
    }

    public async Task<Guid> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == _currentUserService.UserId.Value && e.CourseId == request.CourseId, cancellationToken);

        if (enrollment == null)
        {
            throw new ForbiddenAccessException("You must be enrolled in the course to leave a review.");
        }

        var existing = await _context.Reviews
            .FirstOrDefaultAsync(r => r.UserId == _currentUserService.UserId.Value && r.CourseId == request.CourseId, cancellationToken);

        if (existing != null)
        {
            throw new ValidationException(new[] { new ValidationFailure("Comment", "Już dodałeś opinię do tego kursu.") });
        }

        var review = new Review
        {
            UserId = _currentUserService.UserId.Value,
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
                .AnyAsync(r => r.UserId == _currentUserService.UserId.Value && r.CourseId == request.CourseId, cancellationToken);

            if (!alreadyReviewed)
            {
                throw;
            }

            throw new ValidationException(new[] { new ValidationFailure("Comment", "Już dodałeś opinię do tego kursu.") });
        }

        await _courseIndexing.IndexCourseAsync(request.CourseId, cancellationToken);
        return review.Id;
    }
}
