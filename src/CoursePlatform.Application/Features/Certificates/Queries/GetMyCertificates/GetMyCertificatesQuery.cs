using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Certificates.Queries.GetMyCertificates;

public record CertificateDto(
    Guid Id,
    string Number,
    Guid CourseId,
    string CourseTitle,
    DateTime IssuedAt);

public record GetMyCertificatesQuery : IRequest<List<CertificateDto>>;

public class GetMyCertificatesQueryHandler : IRequestHandler<GetMyCertificatesQuery, List<CertificateDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICertificateIssuer _certificateIssuer;

    public GetMyCertificatesQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICertificateIssuer certificateIssuer)
    {
        _context = context;
        _currentUserService = currentUserService;
        _certificateIssuer = certificateIssuer;
    }

    public async Task<List<CertificateDto>> Handle(GetMyCertificatesQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;

        await BackfillCompletedCoursesAsync(userId, cancellationToken);

        return await _context.Certificates
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.IssuedAt)
            .Select(c => new CertificateDto(
                c.Id,
                c.Number,
                c.CourseId,
                c.Course.Title,
                c.IssuedAt))
            .ToListAsync(cancellationToken);
    }

    // Certificates are normally issued on lesson completion (UpdateProgressCommand). This backfill
    // only covers courses completed before that flow existed (e.g. seeded data). One aggregate query
    // finds fully-completed uncertified courses instead of probing each enrolled course individually.
    private async Task BackfillCompletedCoursesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var certifiedCourseIds = await _context.Certificates
            .Where(c => c.UserId == userId)
            .Select(c => c.CourseId)
            .ToListAsync(cancellationToken);

        var completedCourseIds = await _context.Enrollments
            .Where(e => e.UserId == userId && !certifiedCourseIds.Contains(e.CourseId))
            .Select(e => new
            {
                e.CourseId,
                TotalLessons = _context.Lessons.Count(l => l.Module.CourseId == e.CourseId),
                CompletedLessons = _context.LessonProgresses.Count(p =>
                    p.UserId == userId && p.IsCompleted && p.Lesson.Module.CourseId == e.CourseId)
            })
            .Where(x => x.TotalLessons > 0 && x.CompletedLessons == x.TotalLessons)
            .Select(x => x.CourseId)
            .ToListAsync(cancellationToken);

        foreach (var courseId in completedCourseIds)
        {
            await _certificateIssuer.IssueIfCompletedAsync(userId, courseId, cancellationToken);
        }
    }
}
