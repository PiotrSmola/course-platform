using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Features.Certificates.Services;

public class CertificateIssuer : ICertificateIssuer
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notifications;
    private readonly IEmailQueue _emailQueue;
    private readonly ILogger<CertificateIssuer> _logger;

    public CertificateIssuer(
        IApplicationDbContext context,
        INotificationService notifications,
        IEmailQueue emailQueue,
        ILogger<CertificateIssuer> logger)
    {
        _context = context;
        _notifications = notifications;
        _emailQueue = emailQueue;
        _logger = logger;
    }

    public async Task<Certificate?> IssueIfCompletedAsync(Guid userId, Guid courseId, CancellationToken cancellationToken)
    {
        var totalLessons = await _context.Lessons
            .CountAsync(l => l.Module.CourseId == courseId, cancellationToken);

        if (totalLessons == 0) return null;

        var completedLessons = await _context.LessonProgresses
            .CountAsync(p => p.UserId == userId && p.IsCompleted && p.Lesson.Module.CourseId == courseId, cancellationToken);

        if (completedLessons < totalLessons) return null;

        var alreadyIssued = await _context.Certificates
            .AnyAsync(c => c.UserId == userId && c.CourseId == courseId, cancellationToken);

        if (alreadyIssued) return null;

        var certificate = new Certificate
        {
            UserId = userId,
            CourseId = courseId,
            Number = GenerateNumber(),
            IssuedAt = DateTime.UtcNow
        };

        _context.Certificates.Add(certificate);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var issuedConcurrently = await _context.Certificates
                .AnyAsync(c => c.UserId == userId && c.CourseId == courseId, cancellationToken);

            if (!issuedConcurrently) throw;
            return null;
        }

        _logger.LogInformation("Certificate {Number} issued for user {UserId}, course {CourseId}.",
            certificate.Number, userId, courseId);

        try
        {
            var courseTitle = await _context.Courses
                .Where(c => c.Id == courseId)
                .Select(c => c.Title)
                .FirstOrDefaultAsync(cancellationToken) ?? "kurs";

            var holder = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new { u.Email, u.FirstName })
                .FirstOrDefaultAsync(cancellationToken);

            if (holder?.Email != null)
            {
                var (subject, html) = EmailTemplates.CertificateIssued(holder.FirstName, courseTitle, certificate.Number);
                _emailQueue.Enqueue(new EmailMessage(holder.Email, subject, html));
            }

            await _notifications.NotifyUserAsync(
                userId,
                new UserNotification(
                    "certificate-issued",
                    "Certyfikat wystawiony",
                    $"Gratulacje! Ukończyłeś kurs „{courseTitle}” — certyfikat czeka w Twoim profilu.",
                    "/profile"),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send certificate notification for {Number}.", certificate.Number);
        }

        return certificate;
    }

    private static string GenerateNumber() =>
        $"CERT-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}";
}
