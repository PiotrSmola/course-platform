using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Features.Lessons.Commands.CreateLesson;

public record CreateLessonCommand(
    Guid CourseId,
    Guid ModuleId,
    string Title,
    string? Description,
    string VideoUrl,
    int Duration,
    int Order) : IRequest<Guid>;

public class CreateLessonCommandValidator : AbstractValidator<CreateLessonCommand>
{
    public CreateLessonCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.ModuleId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.VideoUrl).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Duration).GreaterThan(0);
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
    }
}

public class CreateLessonCommandHandler : IRequestHandler<CreateLessonCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CreateLessonCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _currentUserService = currentUserService;
        _userManager = userManager;
    }

    public async Task<Guid> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
    {
        await CourseAccessHelper.GetManagedModuleAsync(
            _context, _userManager, _currentUserService, request.CourseId, request.ModuleId, cancellationToken);

        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            ModuleId = request.ModuleId,
            Title = request.Title,
            Description = request.Description,
            VideoUrl = request.VideoUrl,
            Duration = request.Duration,
            Order = request.Order,
            CreatedAt = DateTime.UtcNow
        };

        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync(cancellationToken);
        return lesson.Id;
    }
}
