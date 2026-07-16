using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Admin.Queries.GetReviews;

public record AdminReviewDto(
    Guid Id,
    Guid CourseId,
    string CourseTitle,
    int Rating,
    string Comment,
    string AuthorName,
    DateTime CreatedAt);

public record GetAdminReviewsQuery : IRequest<List<AdminReviewDto>>;

public class GetAdminReviewsQueryHandler : IRequestHandler<GetAdminReviewsQuery, List<AdminReviewDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAdminReviewsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdminReviewDto>> Handle(GetAdminReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _context.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Course)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return reviews.Select(r => new AdminReviewDto(
            r.Id,
            r.CourseId,
            r.Course.Title,
            r.Rating,
            r.Comment,
            $"{r.User.FirstName} {r.User.LastName}",
            r.CreatedAt)).ToList();
    }
}
