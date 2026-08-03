using FluentValidation;
using FluentValidation.Results;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoursePlatform.Application.Features.Trials.Commands.StartTrial;

public record StartTrialCommand(Guid CourseId) : IRequest<TrialAccessDto>;

public class StartTrialCommandValidator : AbstractValidator<StartTrialCommand>
{
    public StartTrialCommandValidator()
    {
        RuleFor(command => command.CourseId).NotEmpty();
    }
}

public class StartTrialCommandHandler : IRequestHandler<StartTrialCommand, TrialAccessDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public StartTrialCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<TrialAccessDto> Handle(StartTrialCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;
        var trialAccesses = _context.TrialAccesses;
        if (await trialAccesses.AnyAsync(access => access.UserId == userId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("CourseId", "Trial has already been used.")
            });
        }

        if (await CourseAccessHelper.HasActiveSubscriptionAsync(_context, userId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("CourseId", "An active subscription already gives access to this course.")
            });
        }

        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(course => course.Id == request.CourseId, cancellationToken);

        if (course == null)
        {
            throw new NotFoundException($"Course {request.CourseId} not found.");
        }

        if (course.Status != CourseStatus.Published || course.Price <= 0 || course.InstructorId == userId)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("CourseId", "This course is not eligible for a trial.")
            });
        }

        if (await _context.Enrollments.AnyAsync(
                enrollment => enrollment.UserId == userId && enrollment.CourseId == request.CourseId,
                cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("CourseId", "You already have this course.")
            });
        }

        var lessons = await _context.Lessons
            .AsNoTracking()
            .Where(lesson => lesson.Module.CourseId == request.CourseId)
            .OrderBy(lesson => lesson.Module.Order)
            .ThenBy(lesson => lesson.Order)
            .Take(2)
            .Select(lesson => lesson.Id)
            .ToListAsync(cancellationToken);

        if (lessons.Count < 2)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("CourseId", "This course does not contain enough lessons for a trial.")
            });
        }

        var access = new TrialAccess
        {
            UserId = userId,
            CourseId = request.CourseId,
            ActivatedAt = DateTime.UtcNow
        };

        trialAccesses.Add(access);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            if (await trialAccesses.AnyAsync(existing => existing.UserId == userId, cancellationToken))
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure("CourseId", "Trial has already been used.")
                });
            }

            throw;
        }

        return new TrialAccessDto(access.CourseId, access.ActivatedAt, lessons[0], lessons[1]);
    }
}
