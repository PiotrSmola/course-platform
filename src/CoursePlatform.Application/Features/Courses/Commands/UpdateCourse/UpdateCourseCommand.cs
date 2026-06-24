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
    string ThumbnailUrl) : IRequest;

public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCourseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (course == null)
        {
            throw new NotFoundException($"Course {request.Id} not found.");
        }

        if (course.InstructorId != _currentUserService.UserId.Value)
        {
            throw new ForbiddenAccessException("You are not the instructor of this course.");
        }

        course.Title = request.Title;
        course.Description = request.Description;
        course.ShortDescription = request.ShortDescription;
        course.Price = request.Price;
        course.Level = request.Level;
        course.Status = request.Status;
        course.ThumbnailUrl = request.ThumbnailUrl;
        course.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
