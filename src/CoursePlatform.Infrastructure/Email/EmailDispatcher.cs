using CoursePlatform.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoursePlatform.Infrastructure.Email;

public sealed class EmailDispatcher : BackgroundService
{
    private const int MaxAttempts = 3;

    private readonly ChannelEmailQueue _queue;
    private readonly IEmailSender _sender;
    private readonly ILogger<EmailDispatcher> _logger;

    public EmailDispatcher(ChannelEmailQueue queue, IEmailSender sender, ILogger<EmailDispatcher> logger)
    {
        _queue = queue;
        _sender = sender;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            await SendWithRetryAsync(message, stoppingToken);
        }
    }

    private async Task SendWithRetryAsync(EmailMessage message, CancellationToken stoppingToken)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                await _sender.SendAsync(message, stoppingToken);
                _logger.LogInformation("Email '{Subject}' sent to {To}.", message.Subject, message.To);
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                if (attempt == MaxAttempts)
                {
                    _logger.LogWarning(ex, "Email '{Subject}' to {To} failed after {Attempts} attempts. Dropping.",
                        message.Subject, message.To, MaxAttempts);
                    return;
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }
}
