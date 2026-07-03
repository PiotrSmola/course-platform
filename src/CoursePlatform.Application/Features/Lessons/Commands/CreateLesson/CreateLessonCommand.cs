using FluentValidation;
using MediatR;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Features.Lessons.Commands.CreateLesson;

public record CreateLessonCommand(
    Guid CourseId,
    Guid ModuleId,
    string Title,
    string? Description,
    int Duration,
    int Order) : IRequest<Guid>;

public class CreateLessonCommandValidator : AbstractValidator<CreateLessonCommand>
{
    public CreateLessonCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.ModuleId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Duration).GreaterThan(0);
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
    }
}

public class CreateLessonCommandHandler : IRequestHandler<CreateLessonCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateLessonCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
    {
        await CourseAccessHelper.GetManagedModuleAsync(
            _context, _currentUserService, request.CourseId, request.ModuleId, cancellationToken);

        var lesson = new Lesson
        {
            ModuleId = request.ModuleId,
            Title = request.Title,
            Description = request.Description,
            Duration = request.Duration,
            Order = request.Order
        };

        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync(cancellationToken);
        return lesson.Id;
    }
}
