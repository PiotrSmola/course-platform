using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Features.Enrollments.Commands.Enroll;

public record EnrollCommand(Guid CourseId) : IRequest<Guid>;

public class EnrollCommandHandler : IRequestHandler<EnrollCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public EnrollCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(EnrollCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);
        if (course == null)
        {
            throw new NotFoundException($"Course {request.CourseId} not found.");
        }

        if (course.Status != Domain.Enums.CourseStatus.Published)
        {
            throw new ForbiddenAccessException("Course is not published.");
        }

        var existing = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == _currentUserService.UserId.Value && e.CourseId == request.CourseId, cancellationToken);

        if (existing != null)
        {
            throw new ValidationException(new[] { new ValidationFailure("CourseId", "Jesteś już zapisany na ten kurs.") });
        }

        var enrollment = new Enrollment
        {
            UserId = _currentUserService.UserId.Value,
            CourseId = request.CourseId,
            EnrolledAt = DateTime.UtcNow
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync(cancellationToken);
        return enrollment.Id;
    }
}
