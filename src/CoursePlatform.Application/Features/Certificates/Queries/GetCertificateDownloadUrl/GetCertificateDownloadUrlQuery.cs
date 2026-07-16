using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Certificates.Queries.GetCertificateDownloadUrl;

public record CertificateDownloadDto(string Url);

public record GetCertificateDownloadUrlQuery(Guid CertificateId) : IRequest<CertificateDownloadDto>;

public class GetCertificateDownloadUrlQueryValidator : AbstractValidator<GetCertificateDownloadUrlQuery>
{
    public GetCertificateDownloadUrlQueryValidator()
    {
        RuleFor(x => x.CertificateId).NotEmpty();
    }
}

public class GetCertificateDownloadUrlQueryHandler : IRequestHandler<GetCertificateDownloadUrlQuery, CertificateDownloadDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICertificatePdfGenerator _pdfGenerator;
    private readonly IFileStorageService _fileStorage;

    public GetCertificateDownloadUrlQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICertificatePdfGenerator pdfGenerator,
        IFileStorageService fileStorage)
    {
        _context = context;
        _currentUserService = currentUserService;
        _pdfGenerator = pdfGenerator;
        _fileStorage = fileStorage;
    }

    public async Task<CertificateDownloadDto> Handle(GetCertificateDownloadUrlQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var certificate = await _context.Certificates
            .Include(c => c.User)
            .Include(c => c.Course).ThenInclude(course => course.Instructor)
            .FirstOrDefaultAsync(c => c.Id == request.CertificateId, cancellationToken);

        if (certificate == null)
        {
            throw new NotFoundException($"Certificate {request.CertificateId} not found.");
        }

        if (certificate.UserId != _currentUserService.UserId.Value)
        {
            throw new ForbiddenAccessException("You do not own this certificate.");
        }

        if (string.IsNullOrEmpty(certificate.PdfObjectKey))
        {
            var pdf = _pdfGenerator.Generate(new CertificateData(
                certificate.Number,
                $"{certificate.User.FirstName} {certificate.User.LastName}",
                certificate.Course.Title,
                $"{certificate.Course.Instructor.FirstName} {certificate.Course.Instructor.LastName}",
                certificate.IssuedAt));

            var objectKey = ObjectKeyBuilder.CertificatePdfObjectKey(certificate.Id);
            await _fileStorage.UploadAsync(objectKey, pdf, "application/pdf", cancellationToken);

            certificate.PdfObjectKey = objectKey;
            certificate.MarkUpdated();
            await _context.SaveChangesAsync(cancellationToken);
        }

        var url = await _fileStorage.GetPresignedDownloadUrlAsync(
            certificate.PdfObjectKey!,
            TimeSpan.FromMinutes(15),
            cancellationToken);

        return new CertificateDownloadDto(url);
    }
}
