using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.LearningPaths.Queries.GetLearningPaths;

public record GetLearningPathsQuery : IRequest<LearningPathsVm>;

public class GetLearningPathsQueryHandler : IRequestHandler<GetLearningPathsQuery, LearningPathsVm>
{
    private readonly IApplicationDbContext _context;
    private readonly IAppCache _cache;

    public GetLearningPathsQueryHandler(IApplicationDbContext context, IAppCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<LearningPathsVm> Handle(GetLearningPathsQuery request, CancellationToken cancellationToken)
    {
        return await _cache.GetOrCreateAsync(
            "cp:learning-paths",
            TimeSpan.FromMinutes(15),
            async ct => await LoadAsync(ct),
            cancellationToken: cancellationToken);
    }

    private async Task<LearningPathsVm> LoadAsync(CancellationToken cancellationToken)
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