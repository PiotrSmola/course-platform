using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Certificates.Queries.VerifyCertificate;

public record CertificateVerificationDto(
    string Number,
    string HolderName,
    string CourseTitle,
    string InstructorName,
    DateTime IssuedAt);

public record VerifyCertificateQuery(string Number) : IRequest<CertificateVerificationDto>;

public class VerifyCertificateQueryValidator : AbstractValidator<VerifyCertificateQuery>
{
    public VerifyCertificateQueryValidator()
    {
        RuleFor(x => x.Number).NotEmpty().MaximumLength(64);
    }
}

public class VerifyCertificateQueryHandler : IRequestHandler<VerifyCertificateQuery, CertificateVerificationDto>
{
    private readonly IApplicationDbContext _context;

    public VerifyCertificateQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CertificateVerificationDto> Handle(VerifyCertificateQuery request, CancellationToken cancellationToken)
    {
        var normalized = request.Number.Trim().ToUpperInvariant();

        var raw = await _context.Certificates
            .AsNoTracking()
            .Where(c => c.Number == normalized)
            .Select(c => new
            {
                c.Number,
                c.User.FirstName,
                c.User.LastName,
                CourseTitle = c.Course.Title,
                InstructorFirst = c.Course.Instructor.FirstName,
                InstructorLast = c.Course.Instructor.LastName,
                c.IssuedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (raw == null)
        {
            throw new NotFoundException("Certificate not found.");
        }

        // Public endpoint: mask the holder's surname so a certificate number can't be used to harvest
        // full legal names, while still letting a verifier confirm the holder's identity.
        var holderName = $"{raw.FirstName} {MaskSurname(raw.LastName)}".Trim();

        return new CertificateVerificationDto(
            raw.Number,
            holderName,
            raw.CourseTitle,
            $"{raw.InstructorFirst} {raw.InstructorLast}",
            raw.IssuedAt);
    }

    private static string MaskSurname(string surname) =>
        string.IsNullOrEmpty(surname) ? string.Empty : $"{surname[0]}.";
}
