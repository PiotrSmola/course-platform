using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Common.Options;

namespace CoursePlatform.Application.Common.Helpers;

public static class WaitlistNotifier
{
    public static async Task NotifyPublishedAsync(
        IApplicationDbContext context,
        IEmailQueue emailQueue,
        IOptions<FrontendOptions> frontendOptions,
        Guid courseId,
        string courseTitle,
        CancellationToken cancellationToken)
    {
        var entries = await context.CourseWaitlistEntries
            .Include(e => e.User)
            .Where(e => e.CourseId == courseId && e.NotifiedAt == null)
            .ToListAsync(cancellationToken);

        if (entries.Count == 0)
        {
            return;
        }

        var courseUrl = $"{frontendOptions.Value.BaseUrl.TrimEnd('/')}/courses/{courseId}";

        foreach (var entry in entries)
        {
            var (subject, html) = EmailTemplates.CoursePublishedWaitlist(
                entry.User.FirstName,
                courseTitle,
                courseUrl);
            emailQueue.Enqueue(new EmailMessage(entry.User.Email!, subject, html));
            entry.NotifiedAt = DateTime.UtcNow;
            entry.MarkUpdated();
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
