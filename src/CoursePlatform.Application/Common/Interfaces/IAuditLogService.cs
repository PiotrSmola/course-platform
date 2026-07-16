namespace CoursePlatform.Application.Common.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(
        string action,
        string entityType,
        string entityId,
        string details,
        CancellationToken cancellationToken = default);
}
