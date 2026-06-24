using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Reviews.Queries.GetCourseReviews;

public record ReviewListDto(
    Guid Id,
    int Rating,
    string Comment,
    string AuthorName,
    DateTime CreatedAt);

public record GetCourseReviewsQuery(Guid CourseId) : IRequest<List<ReviewListDto>>;

public class GetCourseReviewsQueryHandler : IRequestHandler<GetCourseReviewsQuery, List<ReviewListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCourseReviewsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReviewListDto>> Handle(GetCourseReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _context.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Where(r => r.CourseId == request.CourseId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return reviews.Select(r => new ReviewListDto(
            r.Id,
            r.Rating,
            r.Comment,
            $"{r.User.FirstName} {r.User.LastName}",
            r.CreatedAt)).ToList();
    }
}
