using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Admin.Commands.UpdateCourseStatus;

public record UpdateCourseStatusCommand(Guid CourseId, Domain.Enums.CourseStatus Status) : IRequest;

public class UpdateCourseStatusCommandValidator : AbstractValidator<UpdateCourseStatusCommand>
{
    public UpdateCourseStatusCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}

public class UpdateCourseStatusCommandHandler : IRequestHandler<UpdateCourseStatusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICourseIndexingService _courseIndexing;
    private readonly IAppCache _cache;

    public UpdateCourseStatusCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, ICourseIndexingService courseIndexing, IAppCache cache)
    {
        _context = context;
        _currentUserService = currentUserService;
        _courseIndexing = courseIndexing;
        _cache = cache;
    }

    public async Task Handle(UpdateCourseStatusCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdmin)
        {
            throw new ForbiddenAccessException("Admin access required.");
        }

        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);

        if (course == null)
        {
            throw new NotFoundException($"Course {request.CourseId} not found.");
        }

        course.Status = request.Status;
        course.MarkUpdated();
        await _context.SaveChangesAsync(cancellationToken);
        await _courseIndexing.IndexCourseAsync(course.Id, cancellationToken);
        await _cache.InvalidateTagAsync("courses", cancellationToken);
    }
}
