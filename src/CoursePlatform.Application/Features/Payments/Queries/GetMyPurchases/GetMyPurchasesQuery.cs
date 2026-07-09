using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Payments.Queries.GetMyPurchases;

public record PurchaseDto(
    Guid CourseId,
    string CourseTitle,
    decimal Amount,
    string Currency,
    DateTime CompletedAt);

public record GetMyPurchasesQuery(int Limit = 5) : IRequest<List<PurchaseDto>>;

public class GetMyPurchasesQueryValidator : AbstractValidator<GetMyPurchasesQuery>
{
    public GetMyPurchasesQueryValidator()
    {
        RuleFor(x => x.Limit).InclusiveBetween(1, 100);
    }
}

public class GetMyPurchasesQueryHandler : IRequestHandler<GetMyPurchasesQuery, List<PurchaseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyPurchasesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<PurchaseDto>> Handle(GetMyPurchasesQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new ForbiddenAccessException("User not authenticated.");
        }

        var limit = Math.Clamp(request.Limit, 1, 100);

        return await _context.Payments
            .AsNoTracking()
            .Where(p => p.UserId == _currentUserService.UserId.Value
                && p.Status == PaymentStatus.Completed
                && p.CompletedAt != null)
            .OrderByDescending(p => p.CompletedAt)
            .Take(limit)
            .Select(p => new PurchaseDto(
                p.CourseId,
                p.Course.Title,
                p.Amount,
                p.Currency,
                p.CompletedAt!.Value))
            .ToListAsync(cancellationToken);
    }
}
