using System.Threading.Channels;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Infrastructure.Email;

public sealed class ChannelEmailQueue : IEmailQueue
{
    private readonly Channel<EmailMessage> _channel = Channel.CreateUnbounded<EmailMessage>();

    public ChannelReader<EmailMessage> Reader => _channel.Reader;

    public void Enqueue(EmailMessage message)
    {
        _channel.Writer.TryWrite(message);
    }
}
