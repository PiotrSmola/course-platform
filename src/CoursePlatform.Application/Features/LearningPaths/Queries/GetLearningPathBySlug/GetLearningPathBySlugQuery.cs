using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Exceptions;

namespace CoursePlatform.Application.Features.LearningPaths.Queries.GetLearningPathBySlug;

public record GetLearningPathBySlugQuery(string Slug) : IRequest<LearningPathDetailsDto>;

public class GetLearningPathBySlugQueryHandler : IRequestHandler<GetLearningPathBySlugQuery, LearningPathDetailsDto>
{
    private readonly IApplicationDbContext _context;

    public GetLearningPathBySlugQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LearningPathDetailsDto> Handle(GetLearningPathBySlugQuery request, CancellationToken cancellationToken)
    {
        var path = await _context.LearningPaths
            .AsNoTracking()
            .Include(p => p.PathCourses)
            .ThenInclude(pc => pc.Course)
            .ThenInclude(c => c.Instructor)
            .FirstOrDefaultAsync(p => p.Slug == request.Slug && p.IsPublished, cancellationToken);

        if (path == null)
        {
            throw new NotFoundException($"Learning path '{request.Slug}' not found.");
        }

        var courses = path.PathCourses
            .OrderBy(pc => pc.Order)
            .Select(pc => new LearningPathCourseItemDto(
                pc.Id,
                pc.Order,
                pc.IsOptional,
                pc.Course.Id,
                pc.Course.Title,
                pc.Course.ShortDescription,
                pc.Course.Price,
                pc.Course.Level,
                pc.Course.ThumbnailUrl,
                pc.Course.Language,
                $"{pc.Course.Instructor.FirstName} {pc.Course.Instructor.LastName}"))
            .ToList();

        return new LearningPathDetailsDto(
            path.Id,
            path.Title,
            path.Slug,
            path.ShortDescription,
            path.Description,
            path.DifficultyLevel,
            path.EstimatedHours,
            path.ThumbnailUrl,
            path.CreatedAt,
            courses);
    }
}