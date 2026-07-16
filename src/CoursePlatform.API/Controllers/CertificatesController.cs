using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoursePlatform.Application.Features.Certificates.Queries.GetCertificateDownloadUrl;
using CoursePlatform.Application.Features.Certificates.Queries.GetMyCertificates;
using CoursePlatform.Application.Features.Certificates.Queries.VerifyCertificate;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/certificates")]
[EnableRateLimiting("api")]
public class CertificatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CertificatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<List<CertificateDto>>> GetMyCertificates(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyCertificatesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("verify/{number}")]
    [AllowAnonymous]
    public async Task<ActionResult<CertificateVerificationDto>> Verify(string number, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new VerifyCertificateQuery(number), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/download-url")]
    [Authorize]
    public async Task<ActionResult<CertificateDownloadDto>> GetDownloadUrl(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCertificateDownloadUrlQuery(id), cancellationToken);
        return Ok(result);
    }
}
