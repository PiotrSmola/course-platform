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

        var enrolledCourseIds = await _context.Enrollments
            .Where(e => e.UserId == userId)
            .Select(e => e.CourseId)
            .ToListAsync(cancellationToken);

        var certifiedCourseIds = await _context.Certificates
            .Where(c => c.UserId == userId)
            .Select(c => c.CourseId)
            .ToListAsync(cancellationToken);

        foreach (var courseId in enrolledCourseIds.Except(certifiedCourseIds))
        {
            await _certificateIssuer.IssueIfCompletedAsync(userId, courseId, cancellationToken);
        }

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
}
