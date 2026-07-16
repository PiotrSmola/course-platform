using Microsoft.AspNetCore.SignalR;
using CoursePlatform.API.Hubs;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Admin.Search;

namespace CoursePlatform.API.Services;

public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyReindexAsync(ReindexJobState state, CancellationToken cancellationToken)
    {
        return _hubContext.Clients.Group("admins").SendAsync("reindex", state, cancellationToken);
    }

    public Task NotifyUserAsync(Guid userId, UserNotification notification, CancellationToken cancellationToken)
    {
        return _hubContext.Clients.Group($"user:{userId}").SendAsync("notification", notification, cancellationToken);
    }
}
