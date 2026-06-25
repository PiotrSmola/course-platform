using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.LearningPaths.Queries.GetLearningPaths;

public record GetLearningPathsQuery : IRequest<LearningPathsVm>;

public class GetLearningPathsQueryHandler : IRequestHandler<GetLearningPathsQuery, LearningPathsVm>
{
    private readonly IApplicationDbContext _context;

    public GetLearningPathsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LearningPathsVm> Handle(GetLearningPathsQuery request, CancellationToken cancellationToken)
    {
        var paths = await _context.LearningPaths
            .AsNoTracking()
            .Where(p => p.IsPublished)
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new LearningPathListItemDto(
                p.Id,
                p.Title,
                p.Slug,
                p.ShortDescription,
                p.DifficultyLevel,
                p.EstimatedHours,
                p.ThumbnailUrl,
                p.PathCourses.Count))
            .ToListAsync(cancellationToken);

        return new LearningPathsVm(paths);
    }
}