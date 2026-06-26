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

    public CreateReviewCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IHtmlSanitizer htmlSanitizer)
    {
        _context = context;
        _currentUserService = currentUserService;
        _htmlSanitizer = htmlSanitizer;
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

        if (request.Rating < 1 || request.Rating > 5)
        {
            throw new ValidationException(new[] { new ValidationFailure("Rating", "Ocena musi być w zakresie 1-5.") });
        }

        var review = new Review
        {
            Id = Guid.NewGuid(),
            UserId = _currentUserService.UserId.Value,
            CourseId = request.CourseId,
            Rating = request.Rating,
            Comment = _htmlSanitizer.Sanitize(request.Comment),
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync(cancellationToken);
        return review.Id;
    }
}
