using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Common.Interfaces;

public interface ICertificateIssuer
{
    Task<Certificate?> IssueIfCompletedAsync(Guid userId, Guid courseId, CancellationToken cancellationToken);
}
