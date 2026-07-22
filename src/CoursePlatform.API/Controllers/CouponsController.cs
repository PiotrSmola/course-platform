using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoursePlatform.Application.Common.Authorization;
using CoursePlatform.Application.Features.Coupons;
using CoursePlatform.Application.Features.Coupons.Commands.CreateCoupon;
using CoursePlatform.Application.Features.Coupons.Commands.DeleteCoupon;
using CoursePlatform.Application.Features.Coupons.Commands.UpdateCoupon;
using CoursePlatform.Application.Features.Coupons.Queries.GetCoupons;
using CoursePlatform.Application.Features.Coupons.Queries.PreviewCoupon;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/coupons")]
[EnableRateLimiting("api")]
public class CouponsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CouponsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("preview")]
    [Authorize]
    public async Task<ActionResult<CouponPreviewDto>> Preview(
        [FromBody] PreviewCouponQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<ActionResult<IReadOnlyList<CouponListItemDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCouponsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] CreateCouponRequest body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateCouponCommand(
                body.Code,
                body.DiscountType,
                body.Value,
                body.StartsAt,
                body.ExpiresAt,
                body.MaxRedemptions,
                body.IsActive,
                body.CourseIds),
            cancellationToken);
        return Ok(result);
    }

    [HttpPut("{couponId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<ActionResult> Update(
        Guid couponId,
        [FromBody] UpdateCouponRequest body,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateCouponCommand(
                couponId,
                body.DiscountType,
                body.Value,
                body.StartsAt,
                body.ExpiresAt,
                body.MaxRedemptions,
                body.IsActive,
                body.CourseIds),
            cancellationToken);
        return NoContent();
    }

    [HttpDelete("{couponId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<ActionResult> Delete(Guid couponId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCouponCommand(couponId), cancellationToken);
        return NoContent();
    }
}
