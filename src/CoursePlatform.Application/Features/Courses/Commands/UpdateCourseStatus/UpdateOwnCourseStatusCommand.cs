using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Options;
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
    private readonly IEmailQueue _emailQueue;
    private readonly IOptions<FrontendOptions> _frontendOptions;

    public UpdateOwnCourseStatusCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICourseIndexingService courseIndexing,
        IAppCache cache,
        IEmailQueue emailQueue,
        IOptions<FrontendOptions> frontendOptions)
    {
        _context = context;
        _currentUserService = currentUserService;
        _courseIndexing = courseIndexing;
        _cache = cache;
        _emailQueue = emailQueue;
        _frontendOptions = frontendOptions;
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

        var previousStatus = course.Status;
        course.Status = request.Status;
        course.MarkUpdated();
        await _context.SaveChangesAsync(cancellationToken);

        if (request.Status == CourseStatus.Published && previousStatus != CourseStatus.Published)
        {
            await WaitlistNotifier.NotifyPublishedAsync(
                _context,
                _emailQueue,
                _frontendOptions,
                course.Id,
                course.Title,
                cancellationToken);
        }

        await _courseIndexing.IndexCourseAsync(course.Id, cancellationToken);
        await _cache.InvalidateTagAsync("courses", cancellationToken);
    }
}
