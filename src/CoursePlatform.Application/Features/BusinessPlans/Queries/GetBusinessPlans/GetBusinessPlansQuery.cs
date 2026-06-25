using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.BusinessPlans.Queries.GetBusinessPlans;

public record GetBusinessPlansQuery : IRequest<BusinessPlansVm>;

public class GetBusinessPlansQueryHandler : IRequestHandler<GetBusinessPlansQuery, BusinessPlansVm>
{
    private readonly IApplicationDbContext _context;

    public GetBusinessPlansQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BusinessPlansVm> Handle(GetBusinessPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await _context.BusinessPlans
            .AsNoTracking()
            .Where(p => p.IsPublished)
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new BusinessPlanDto(
                p.Id,
                p.Name,
                p.Slug,
                p.ShortDescription,
                p.Price,
                p.Currency,
                p.BillingPeriod,
                p.PriceLabel,
                p.CallToActionText,
                p.CallToActionUrl,
                p.IsFeatured,
                p.DisplayOrder,
                p.Features
                    .OrderBy(f => f.DisplayOrder)
                    .Select(f => new BusinessPlanFeatureDto(f.Id, f.Text, f.DisplayOrder))
                    .ToList()))
            .ToListAsync(cancellationToken);

        return new BusinessPlansVm(plans);
    }
}