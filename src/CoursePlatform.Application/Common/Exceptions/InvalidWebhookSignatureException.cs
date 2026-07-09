namespace CoursePlatform.Application.Common.Exceptions;

public class InvalidWebhookSignatureException : Exception
{
    public InvalidWebhookSignatureException(string message) : base(message) { }
}
