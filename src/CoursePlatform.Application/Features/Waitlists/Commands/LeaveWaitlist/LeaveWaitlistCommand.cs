using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Exceptions;

namespace CoursePlatform.Application.Features.Waitlists.Commands.LeaveWaitlist;

public record LeaveWaitlistCommand(Guid CourseId) : IRequest;

public class LeaveWaitlistCommandValidator : AbstractValidator<LeaveWaitlistCommand>
{
    public LeaveWaitlistCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
    }
}

public class LeaveWaitlistCommandHandler : IRequestHandler<LeaveWaitlistCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public LeaveWaitlistCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(LeaveWaitlistCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var entry = await _context.CourseWaitlistEntries
            .FirstOrDefaultAsync(
                e => e.UserId == _currentUserService.UserId.Value && e.CourseId == request.CourseId,
                cancellationToken);

        if (entry == null)
        {
            throw new NotFoundException($"Waitlist entry for course {request.CourseId} not found.");
        }

        _context.CourseWaitlistEntries.Remove(entry);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
