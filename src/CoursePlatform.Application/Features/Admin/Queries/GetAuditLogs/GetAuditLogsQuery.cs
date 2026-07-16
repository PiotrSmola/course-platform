using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Admin.Queries.GetAuditLogs;

public record AuditLogDto(
    Guid Id,
    DateTime CreatedAt,
    string Action,
    string EntityType,
    string EntityId,
    string Details,
    string? AdminEmail);

public record GetAuditLogsQuery(int PageNumber = 1, int PageSize = 50) : IRequest<PagedAuditLogsDto>;

public record PagedAuditLogsDto(
    IReadOnlyList<AuditLogDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PagedAuditLogsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAuditLogsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PagedAuditLogsDto> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdmin)
        {
            throw new ForbiddenAccessException("Admin access required.");
        }

        var query = _context.AuditLogs
            .AsNoTracking()
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AuditLogDto(
                a.Id,
                a.CreatedAt,
                a.Action,
                a.EntityType,
                a.EntityId,
                a.Details,
                a.User == null ? null : a.User.Email))
            .ToListAsync(cancellationToken);

        return new PagedAuditLogsDto(items, totalCount, request.PageNumber, request.PageSize);
    }
}
