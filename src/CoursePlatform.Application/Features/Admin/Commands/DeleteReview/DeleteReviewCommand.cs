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

    public DeleteReviewCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, ICourseIndexingService courseIndexing)
    {
        _context = context;
        _currentUserService = currentUserService;
        _courseIndexing = courseIndexing;
    }

    public async Task Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdmin)
        {
            throw new ForbiddenAccessException("Admin access required.");
        }

        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review == null)
        {
            throw new NotFoundException($"Review {request.ReviewId} not found.");
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync(cancellationToken);
        await _courseIndexing.IndexCourseAsync(review.CourseId, cancellationToken);
    }
}
