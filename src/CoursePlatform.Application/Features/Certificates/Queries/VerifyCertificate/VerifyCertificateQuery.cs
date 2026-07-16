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

        var certificate = await _context.Certificates
            .AsNoTracking()
            .Where(c => c.Number == normalized)
            .Select(c => new CertificateVerificationDto(
                c.Number,
                $"{c.User.FirstName} {c.User.LastName}",
                c.Course.Title,
                $"{c.Course.Instructor.FirstName} {c.Course.Instructor.LastName}",
                c.IssuedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (certificate == null)
        {
            throw new NotFoundException("Certificate not found.");
        }

        return certificate;
    }
}
