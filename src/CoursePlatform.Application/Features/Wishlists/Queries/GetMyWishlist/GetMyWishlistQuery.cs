using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Helpers;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Wishlists.Queries.GetMyWishlist;

public record WishlistItemDto(
    Guid Id,
    Guid CourseId,
    string CourseTitle,
    string? CourseThumbnailUrl,
    CourseLevel CourseLevel,
    decimal Price,
    DateTime AddedAt);

public record GetMyWishlistQuery : IRequest<List<WishlistItemDto>>;

public class GetMyWishlistQueryHandler : IRequestHandler<GetMyWishlistQuery, List<WishlistItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorage;

    public GetMyWishlistQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorage)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
    }

    public async Task<List<WishlistItemDto>> Handle(GetMyWishlistQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return new List<WishlistItemDto>();
        }

        var items = await _context.WishlistItems
            .AsNoTracking()
            .Include(w => w.Course)
            .Where(w => w.UserId == _currentUserService.UserId.Value && w.Course.Status == CourseStatus.Published)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);

        var result = new List<WishlistItemDto>(items.Count);
        foreach (var item in items)
        {
            result.Add(new WishlistItemDto(
                item.Id,
                item.CourseId,
                item.Course.Title,
                await _fileStorage.GetThumbnailUrlOrNullAsync(item.Course.ThumbnailObjectKey, cancellationToken),
                item.Course.Level,
                item.Course.Price,
                item.CreatedAt));
        }

        return result;
    }
}
