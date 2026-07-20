using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Lessons.Commands.UpdateLesson;

public record UpdateLessonCommand(
    Guid CourseId,
    Guid ModuleId,
    Guid LessonId,
    string Title,
    string? Description,
    int Duration,
    int Order) : IRequest;

public class UpdateLessonCommandValidator : AbstractValidator<UpdateLessonCommand>
{
    public UpdateLessonCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.ModuleId).NotEmpty();
        RuleFor(x => x.LessonId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Duration).GreaterThan(0);
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
    }
}

public class UpdateLessonCommandHandler : IRequestHandler<UpdateLessonCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAppCache _cache;

    public UpdateLessonCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IAppCache cache)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cache = cache;
    }

    public async Task Handle(UpdateLessonCommand request, CancellationToken cancellationToken)
    {
        await CourseAccessHelper.GetManagedModuleAsync(
            _context, _currentUserService, request.CourseId, request.ModuleId, cancellationToken);

        var lesson = await _context.Lessons
            .FirstOrDefaultAsync(l => l.Id == request.LessonId && l.ModuleId == request.ModuleId, cancellationToken);

        if (lesson == null)
        {
            throw new NotFoundException($"Lesson {request.LessonId} not found.");
        }

        lesson.Title = request.Title;
        lesson.Description = request.Description;
        lesson.Duration = request.Duration;
        lesson.Order = request.Order;
        lesson.MarkUpdated();

        await _context.SaveChangesAsync(cancellationToken);
        await _cache.InvalidateTagAsync("courses", cancellationToken);
    }
}
