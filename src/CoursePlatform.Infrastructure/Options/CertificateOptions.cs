namespace CoursePlatform.Infrastructure.Options;

public sealed class CertificateOptions
{
    public string VerificationBaseUrl { get; init; } = "http://localhost:5173/certificates/verify";
}
