using CoursePlatform.Application.Features.Admin.Search;

namespace CoursePlatform.Application.Common.Interfaces;

public record UserNotification(string Type, string Title, string Message, string? Link);

public interface INotificationService
{
    Task NotifyReindexAsync(ReindexJobState state, CancellationToken cancellationToken);
    Task NotifyUserAsync(Guid userId, UserNotification notification, CancellationToken cancellationToken);
}
