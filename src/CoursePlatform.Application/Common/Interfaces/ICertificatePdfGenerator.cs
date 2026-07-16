namespace CoursePlatform.Application.Common.Interfaces;

public record CertificateData(
    string Number,
    string HolderName,
    string CourseTitle,
    string InstructorName,
    DateTime IssuedAt);

public interface ICertificatePdfGenerator
{
    byte[] Generate(CertificateData data);
}
