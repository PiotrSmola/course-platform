using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Admin.Commands.DeleteReview;

public record DeleteReviewCommand(Guid ReviewId) : IRequest;

public class DeleteReviewCommandValidator : AbstractValidator<DeleteReviewCommand>
{
    public DeleteReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty();
    }
}

public class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICourseIndexingService _courseIndexing;
    private readonly IAppCache _cache;
    private readonly IAuditLogService _auditLog;

    public DeleteReviewCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICourseIndexingService courseIndexing,
        IAppCache cache,
        IAuditLogService auditLog)
    {
        _context = context;
        _currentUserService = currentUserService;
        _courseIndexing = courseIndexing;
        _cache = cache;
        _auditLog = auditLog;
    }

    public async Task Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdmin)
        {
            throw new ForbiddenAccessException("Admin access required.");
        }

        var review = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Course)
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review == null)
        {
            throw new NotFoundException($"Review {request.ReviewId} not found.");
        }

        var courseId = review.CourseId;
        var courseTitle = review.Course.Title;
        var authorEmail = review.User.Email;

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync(cancellationToken);
        await _courseIndexing.IndexCourseAsync(courseId, cancellationToken);
        await _cache.InvalidateTagAsync("courses", cancellationToken);

        await _auditLog.LogAsync(
            "DeleteReview",
            "Review",
            request.ReviewId.ToString(),
            $"Review by '{authorEmail}' removed from course '{courseTitle}'.",
            cancellationToken);
    }
}
