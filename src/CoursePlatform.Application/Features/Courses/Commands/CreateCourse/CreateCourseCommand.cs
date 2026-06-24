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
    string ThumbnailUrl) : IRequest<Guid>;

public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateCourseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            ShortDescription = request.ShortDescription,
            Price = request.Price,
            Level = request.Level,
            Status = CourseStatus.Draft,
            ThumbnailUrl = request.ThumbnailUrl,
            InstructorId = _currentUserService.UserId.Value,
            CreatedAt = DateTime.UtcNow
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync(cancellationToken);
        return course.Id;
    }
}
