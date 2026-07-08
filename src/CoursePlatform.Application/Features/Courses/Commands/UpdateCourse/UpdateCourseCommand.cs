using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Courses.Commands.UpdateCourse;

public record UpdateCourseCommand(
    Guid Id,
    string Title,
    string Description,
    string ShortDescription,
    decimal Price,
    CourseLevel Level,
    CourseStatus Status,
    string Language,
    List<Guid> CategoryIds,
    List<Guid> TechnologyIds) : IRequest;

public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHtmlSanitizer _htmlSanitizer;
    private readonly ICourseIndexingService _courseIndexing;

    public UpdateCourseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IHtmlSanitizer htmlSanitizer, ICourseIndexingService courseIndexing)
    {
        _context = context;
        _currentUserService = currentUserService;
        _htmlSanitizer = htmlSanitizer;
        _courseIndexing = courseIndexing;
    }

    public async Task Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var course = await _context.Courses
            .Include(c => c.Categories)
            .Include(c => c.Technologies)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (course == null)
        {
            throw new NotFoundException($"Course {request.Id} not found.");
        }

        if (!_currentUserService.IsAdmin && course.InstructorId != _currentUserService.UserId.Value)
        {
            throw new ForbiddenAccessException("You are not the instructor of this course.");
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

        course.Title = request.Title;
        course.Description = _htmlSanitizer.Sanitize(request.Description);
        course.ShortDescription = _htmlSanitizer.Sanitize(request.ShortDescription);
        course.Price = request.Price;
        course.Level = request.Level;
        course.Status = request.Status;
        course.Language = request.Language;
        course.Categories = categories;
        course.Technologies = technologies;
        course.MarkUpdated();

        await _context.SaveChangesAsync(cancellationToken);
        await _courseIndexing.IndexCourseAsync(course.Id, cancellationToken);
    }
}
