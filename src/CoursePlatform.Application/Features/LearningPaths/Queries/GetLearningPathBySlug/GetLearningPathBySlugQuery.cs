using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.LearningPaths.Queries.GetLearningPathBySlug;

public record GetLearningPathBySlugQuery(string Slug) : IRequest<LearningPathDetailsDto>;

public class GetLearningPathBySlugQueryHandler : IRequestHandler<GetLearningPathBySlugQuery, LearningPathDetailsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public GetLearningPathBySlugQueryHandler(IApplicationDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
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

        var pathCourses = path.PathCourses
            .Where(pc => pc.Course.Status == CourseStatus.Published)
            .OrderBy(pc => pc.Order)
            .ToList();

        var courses = new List<LearningPathCourseItemDto>(pathCourses.Count);
        foreach (var pc in pathCourses)
        {
            courses.Add(new LearningPathCourseItemDto(
                pc.Id,
                pc.Order,
                pc.IsOptional,
                pc.Course.Id,
                pc.Course.Title,
                pc.Course.ShortDescription,
                pc.Course.Price,
                pc.Course.Level,
                await _fileStorage.GetThumbnailUrlOrNullAsync(pc.Course.ThumbnailObjectKey, cancellationToken),
                pc.Course.Language,
                $"{pc.Course.Instructor.FirstName} {pc.Course.Instructor.LastName}"));
        }

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