using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Courses.Commands.CreateCourse;

public record CreateCourseCommand(
    string Title,
    string Description,
    string ShortDescription,
    decimal Price,
    CourseLevel Level,
    string ThumbnailUrl,
    string Language,
    List<Guid> CategoryIds,
    List<Guid> TechnologyIds) : IRequest<Guid>;

public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHtmlSanitizer _htmlSanitizer;

    public CreateCourseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IHtmlSanitizer htmlSanitizer)
    {
        _context = context;
        _currentUserService = currentUserService;
        _htmlSanitizer = htmlSanitizer;
    }

    public async Task<Guid> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var categories = await _context.Categories
            .Where(c => request.CategoryIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        if (categories.Count != request.CategoryIds.Count)
        {
            throw new FluentValidation.ValidationException(new[] { new FluentValidation.Results.ValidationFailure("CategoryIds", "One or more categories do not exist.") });
        }

        var technologies = await _context.Technologies
            .Where(t => request.TechnologyIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        if (technologies.Count != request.TechnologyIds.Count)
        {
            throw new FluentValidation.ValidationException(new[] { new FluentValidation.Results.ValidationFailure("TechnologyIds", "One or more technologies do not exist.") });
        }

        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = _htmlSanitizer.Sanitize(request.Description),
            ShortDescription = _htmlSanitizer.Sanitize(request.ShortDescription),
            Price = request.Price,
            Level = request.Level,
            Status = CourseStatus.Draft,
            ThumbnailUrl = request.ThumbnailUrl,
            Language = request.Language,
            InstructorId = _currentUserService.UserId.Value,
            CreatedAt = DateTime.UtcNow,
            Categories = categories,
            Technologies = technologies
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync(cancellationToken);
        return course.Id;
    }
}
