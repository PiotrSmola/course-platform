using FluentValidation;
using MediatR;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Courses.Commands.UpdateCourseStatus;

public record UpdateOwnCourseStatusCommand(Guid CourseId, CourseStatus Status) : IRequest;

public class UpdateOwnCourseStatusCommandValidator : AbstractValidator<UpdateOwnCourseStatusCommand>
{
    public UpdateOwnCourseStatusCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}

public class UpdateOwnCourseStatusCommandHandler : IRequestHandler<UpdateOwnCourseStatusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICourseIndexingService _courseIndexing;
    private readonly IAppCache _cache;

    public UpdateOwnCourseStatusCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICourseIndexingService courseIndexing,
        IAppCache cache)
    {
        _context = context;
        _currentUserService = currentUserService;
        _courseIndexing = courseIndexing;
        _cache = cache;
    }

    public async Task Handle(UpdateOwnCourseStatusCommand request, CancellationToken cancellationToken)
    {
        var course = await CourseAccessHelper.GetManagedCourseAsync(
            _context, _currentUserService, request.CourseId, cancellationToken);

        if (!_currentUserService.IsAdmin &&
            request.Status is not (CourseStatus.Draft or CourseStatus.Published or CourseStatus.Hidden))
        {
            throw new ForbiddenAccessException("Invalid course status.");
        }

        course.Status = request.Status;
        course.MarkUpdated();
        await _context.SaveChangesAsync(cancellationToken);
        await _courseIndexing.IndexCourseAsync(course.Id, cancellationToken);
        await _cache.InvalidateTagAsync("courses", cancellationToken);
    }
}
