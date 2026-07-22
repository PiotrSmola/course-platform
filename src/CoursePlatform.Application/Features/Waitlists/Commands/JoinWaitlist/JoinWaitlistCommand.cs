using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Waitlists.Commands.JoinWaitlist;

public record JoinWaitlistCommand(Guid CourseId) : IRequest<Guid>;

public class JoinWaitlistCommandValidator : AbstractValidator<JoinWaitlistCommand>
{
    public JoinWaitlistCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
    }
}

public class JoinWaitlistCommandHandler : IRequestHandler<JoinWaitlistCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public JoinWaitlistCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(JoinWaitlistCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);

        if (course == null)
        {
            throw new NotFoundException($"Course {request.CourseId} not found.");
        }

        if (course.Status is not (CourseStatus.Draft or CourseStatus.Hidden))
        {
            throw new ForbiddenAccessException("Waitlist is only available for unpublished courses.");
        }

        var userId = _currentUserService.UserId.Value;

        if (course.InstructorId == userId)
        {
            throw new ForbiddenAccessException("Instructors cannot join the waitlist for their own course.");
        }

        var existing = await _context.CourseWaitlistEntries
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == request.CourseId, cancellationToken);

        if (existing != null)
        {
            return existing.Id;
        }

        var entry = new CourseWaitlistEntry
        {
            UserId = userId,
            CourseId = request.CourseId
        };

        _context.CourseWaitlistEntries.Add(entry);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var alreadyExists = await _context.CourseWaitlistEntries
                .AnyAsync(e => e.UserId == userId && e.CourseId == request.CourseId, cancellationToken);

            if (!alreadyExists)
            {
                throw;
            }

            throw new ValidationException(new[] { new ValidationFailure("CourseId", "Jesteś już na liście oczekujących.") });
        }

        return entry.Id;
    }
}
