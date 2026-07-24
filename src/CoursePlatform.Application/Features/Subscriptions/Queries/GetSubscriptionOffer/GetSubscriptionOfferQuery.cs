using MediatR;
using Microsoft.Extensions.Options;
using CoursePlatform.Application.Common.Options;

namespace CoursePlatform.Application.Features.Subscriptions.Queries.GetSubscriptionOffer;

public record SubscriptionOfferDto(decimal MonthlyPricePln);

public record GetSubscriptionOfferQuery : IRequest<SubscriptionOfferDto>;

public class GetSubscriptionOfferQueryHandler : IRequestHandler<GetSubscriptionOfferQuery, SubscriptionOfferDto>
{
    private readonly SubscriptionOptions _subscriptionOptions;

    public GetSubscriptionOfferQueryHandler(IOptions<SubscriptionOptions> subscriptionOptions)
    {
        _subscriptionOptions = subscriptionOptions.Value;
    }

    public Task<SubscriptionOfferDto> Handle(GetSubscriptionOfferQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new SubscriptionOfferDto(_subscriptionOptions.MonthlyPricePln));
    }
}
