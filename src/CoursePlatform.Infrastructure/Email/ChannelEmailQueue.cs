using System.Threading.Channels;
using CoursePlatform.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CoursePlatform.Infrastructure.Email;

public sealed class ChannelEmailQueue : IEmailQueue
{
    private const int Capacity = 10_000;

    private readonly Channel<EmailMessage> _channel = Channel.CreateBounded<EmailMessage>(
        new BoundedChannelOptions(Capacity)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true
        });

    private readonly ILogger<ChannelEmailQueue> _logger;

    public ChannelEmailQueue(ILogger<ChannelEmailQueue> logger)
    {
        _logger = logger;
    }

    public ChannelReader<EmailMessage> Reader => _channel.Reader;

    public void Enqueue(EmailMessage message)
    {
        if (!_channel.Writer.TryWrite(message))
        {
            _logger.LogWarning("Email queue full or closed — dropped '{Subject}' to {To}.",
                message.Subject, message.To);
        }
    }
}
