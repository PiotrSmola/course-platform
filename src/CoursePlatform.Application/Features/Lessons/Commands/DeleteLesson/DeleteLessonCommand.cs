using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Features.Lessons.Commands.DeleteLesson;

public record DeleteLessonCommand(Guid CourseId, Guid ModuleId, Guid LessonId) : IRequest;

public class DeleteLessonCommandValidator : AbstractValidator<DeleteLessonCommand>
{
    public DeleteLessonCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.ModuleId).NotEmpty();
        RuleFor(x => x.LessonId).NotEmpty();
    }
}

public class DeleteLessonCommandHandler : IRequestHandler<DeleteLessonCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteLessonCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _currentUserService = currentUserService;
        _userManager = userManager;
    }

    public async Task Handle(DeleteLessonCommand request, CancellationToken cancellationToken)
    {
        await CourseAccessHelper.GetManagedModuleAsync(
            _context, _userManager, _currentUserService, request.CourseId, request.ModuleId, cancellationToken);

        var lesson = await _context.Lessons
            .FirstOrDefaultAsync(l => l.Id == request.LessonId && l.ModuleId == request.ModuleId, cancellationToken);

        if (lesson == null)
        {
            throw new NotFoundException($"Lesson {request.LessonId} not found.");
        }

        _context.Lessons.Remove(lesson);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
