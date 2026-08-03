using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Exceptions;
using FluentValidation;
using FluentValidation.Results;

namespace CoursePlatform.Application.Features.Courses.Commands.DeleteCourse;

public record DeleteCourseCommand(Guid Id) : IRequest;

public class DeleteCourseCommandHandler : IRequestHandler<DeleteCourseCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICourseIndexingService _courseIndexing;
    private readonly IAppCache _cache;

    public DeleteCourseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, ICourseIndexingService courseIndexing, IAppCache cache)
    {
        _context = context;
        _currentUserService = currentUserService;
        _courseIndexing = courseIndexing;
        _cache = cache;
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
        var hasGiftPurchases = await _context.GiftPurchases
            .AnyAsync(gift => gift.CourseId == course.Id, cancellationToken);
        if (hasGiftPurchases)
        {
            throw new ValidationException(new[] { new ValidationFailure("", "Cannot delete a course with gift purchase history.") });
        }

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync(cancellationToken);
        await _courseIndexing.DeleteCourseAsync(request.Id, cancellationToken);
        await _cache.InvalidateTagAsync("courses", cancellationToken);
    }
}
