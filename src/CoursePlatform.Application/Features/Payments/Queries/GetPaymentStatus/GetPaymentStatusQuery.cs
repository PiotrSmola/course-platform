using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Payments.Queries.GetPaymentStatus;

public record PaymentStatusDto(
    PaymentStatus Status,
    Guid CourseId,
    string CourseTitle,
    decimal Amount,
    string Currency,
    DateTime? CompletedAt);

public record GetPaymentStatusQuery(string SessionId) : IRequest<PaymentStatusDto>;

public class GetPaymentStatusQueryValidator : AbstractValidator<GetPaymentStatusQuery>
{
    public GetPaymentStatusQueryValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty().MaximumLength(255);
    }
}

public class GetPaymentStatusQueryHandler : IRequestHandler<GetPaymentStatusQuery, PaymentStatusDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPaymentStatusQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaymentStatusDto> Handle(GetPaymentStatusQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var payment = await _context.Payments
            .AsNoTracking()
            .Where(p => p.StripeSessionId == request.SessionId && p.UserId == _currentUserService.UserId.Value)
            .Select(p => new PaymentStatusDto(
                p.Status,
                p.CourseId,
                p.Course.Title,
                p.Amount,
                p.Currency,
                p.CompletedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (payment == null)
        {
            throw new NotFoundException("Payment not found.");
        }

        return payment;
    }
}
