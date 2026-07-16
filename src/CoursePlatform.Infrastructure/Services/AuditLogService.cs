using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Infrastructure.Services;

public sealed class AuditLogService : IAuditLogService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AuditLogService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task LogAsync(
        string action,
        string entityType,
        string entityId,
        string details,
        CancellationToken cancellationToken = default)
    {
        var entry = new AuditLog
        {
            UserId = _currentUserService.UserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details
        };

        _context.AuditLogs.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
