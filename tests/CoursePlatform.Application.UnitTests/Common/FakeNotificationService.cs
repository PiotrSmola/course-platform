using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Admin.Search;

namespace CoursePlatform.Application.UnitTests.Common;

public sealed class FakeNotificationService : INotificationService
{
    public List<ReindexJobState> ReindexStates { get; } = new();
    public List<(Guid UserId, UserNotification Notification)> UserNotifications { get; } = new();

    public Task NotifyReindexAsync(ReindexJobState state, CancellationToken cancellationToken)
    {
        ReindexStates.Add(state);
        return Task.CompletedTask;
    }

    public Task NotifyUserAsync(Guid userId, UserNotification notification, CancellationToken cancellationToken)
    {
        UserNotifications.Add((userId, notification));
        return Task.CompletedTask;
    }
}
