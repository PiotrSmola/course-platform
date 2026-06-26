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
    string ThumbnailUrl,
    string Language,
    List<Guid> CategoryIds,
    List<Guid> TechnologyIds) : IRequest;

public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHtmlSanitizer _htmlSanitizer;

    public UpdateCourseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IHtmlSanitizer htmlSanitizer)
    {
        _context = context;
        _currentUserService = currentUserService;
        _htmlSanitizer = htmlSanitizer;
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

        if (course.InstructorId != _currentUserService.UserId.Value)
        {
            throw new ForbiddenAccessException("You are not the instructor of this course.");
        }

        var categories = await _context.Categories
            .Where(c => request.CategoryIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        var technologies = await _context.Technologies
            .Where(t => request.TechnologyIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        course.Title = request.Title;
        course.Description = _htmlSanitizer.Sanitize(request.Description);
        course.ShortDescription = _htmlSanitizer.Sanitize(request.ShortDescription);
        course.Price = request.Price;
        course.Level = request.Level;
        course.Status = request.Status;
        course.ThumbnailUrl = request.ThumbnailUrl;
        course.Language = request.Language;
        course.Categories = categories;
        course.Technologies = technologies;
        course.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
