using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.UnitTests.Common;

public sealed class FakeEmailQueue : IEmailQueue
{
    public List<EmailMessage> Sent { get; } = new();

    public void Enqueue(EmailMessage message) => Sent.Add(message);
}
