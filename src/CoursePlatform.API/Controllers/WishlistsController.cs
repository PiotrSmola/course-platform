using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoursePlatform.Application.Features.Wishlists.Commands.AddToWishlist;
using CoursePlatform.Application.Features.Wishlists.Commands.RemoveFromWishlist;
using CoursePlatform.Application.Features.Wishlists.Queries.GetMyWishlist;

namespace CoursePlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("api")]
public class WishlistsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WishlistsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<List<WishlistItemDto>>> GetMyWishlist(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyWishlistQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{courseId:guid}")]
    [Authorize]
    public async Task<ActionResult<Guid>> AddToWishlist(Guid courseId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AddToWishlistCommand(courseId), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{courseId:guid}")]
    [Authorize]
    public async Task<IActionResult> RemoveFromWishlist(Guid courseId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveFromWishlistCommand(courseId), cancellationToken);
        return NoContent();
    }
}
