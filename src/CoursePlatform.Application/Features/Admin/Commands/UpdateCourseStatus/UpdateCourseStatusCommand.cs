using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Options;
using CoursePlatform.Domain.Enums;

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
    private readonly IAuditLogService _auditLog;
    private readonly IEmailQueue _emailQueue;
    private readonly IOptions<FrontendOptions> _frontendOptions;

    public UpdateCourseStatusCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICourseIndexingService courseIndexing,
        IAppCache cache,
        IAuditLogService auditLog,
        IEmailQueue emailQueue,
        IOptions<FrontendOptions> frontendOptions)
    {
        _context = context;
        _currentUserService = currentUserService;
        _courseIndexing = courseIndexing;
        _cache = cache;
        _auditLog = auditLog;
        _emailQueue = emailQueue;
        _frontendOptions = frontendOptions;
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

        await _auditLog.LogAsync(
            "UpdateCourseStatus",
            "Course",
            course.Id.ToString(),
            $"Status changed from {previousStatus} to {request.Status} for course '{course.Title}'.",
            cancellationToken);
    }
}
