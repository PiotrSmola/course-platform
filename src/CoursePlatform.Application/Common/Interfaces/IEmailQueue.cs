namespace CoursePlatform.Application.Common.Interfaces;

public record EmailMessage(string To, string Subject, string HtmlBody);

public interface IEmailQueue
{
    void Enqueue(EmailMessage message);
}
