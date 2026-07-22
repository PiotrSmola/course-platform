using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Analytics.Queries.GetAdminRevenueOverview;

public record AdminRevenueOverviewDto(
    decimal CourseSalesRevenue,
    decimal SubscriptionRevenue,
    decimal TotalRevenue,
    int CompletedCoursePayments,
    int PaidSubscriptionInvoices,
    int ActiveSubscriptions);

public record GetAdminRevenueOverviewQuery : IRequest<AdminRevenueOverviewDto>;

public class GetAdminRevenueOverviewQueryHandler
    : IRequestHandler<GetAdminRevenueOverviewQuery, AdminRevenueOverviewDto>
{
    private readonly IApplicationDbContext _context;

    public GetAdminRevenueOverviewQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminRevenueOverviewDto> Handle(
        GetAdminRevenueOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var courseSales = await _context.Payments
            .AsNoTracking()
            .Where(p => p.Status == PaymentStatus.Completed)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Revenue = g.Sum(p => p.Amount),
                Count = g.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        var subscriptionSales = await _context.SubscriptionInvoices
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Revenue = g.Sum(i => i.Amount),
                Count = g.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var activeSubscriptions = await _context.Subscriptions
            .AsNoTracking()
            .CountAsync(
                s => s.CurrentPeriodEnd > now
                     && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.PastDue),
                cancellationToken);

        var courseRevenue = courseSales?.Revenue ?? 0m;
        var subRevenue = subscriptionSales?.Revenue ?? 0m;

        return new AdminRevenueOverviewDto(
            courseRevenue,
            subRevenue,
            courseRevenue + subRevenue,
            courseSales?.Count ?? 0,
            subscriptionSales?.Count ?? 0,
            activeSubscriptions);
    }
}
