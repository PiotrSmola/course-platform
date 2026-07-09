using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Exceptions;

namespace CoursePlatform.Application.Features.Courses.Commands.DeleteCourse;

public record DeleteCourseCommand(Guid Id) : IRequest;

public class DeleteCourseCommandHandler : IRequestHandler<DeleteCourseCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICourseIndexingService _courseIndexing;

    public DeleteCourseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, ICourseIndexingService courseIndexing)
    {
        _context = context;
        _currentUserService = currentUserService;
        _courseIndexing = courseIndexing;
    }

    public async Task Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (course == null)
        {
            throw new NotFoundException($"Course {request.Id} not found.");
        }

        if (!_currentUserService.IsAdmin && course.InstructorId != _currentUserService.UserId.Value)
        {
            throw new ForbiddenAccessException("You are not the instructor of this course.");
        }

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync(cancellationToken);
        await _courseIndexing.DeleteCourseAsync(request.Id, cancellationToken);
    }
}
