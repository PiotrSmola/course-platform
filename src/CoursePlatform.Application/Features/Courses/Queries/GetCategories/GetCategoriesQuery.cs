using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Courses.Queries.GetCategories;

public record CategoryDto(Guid Id, string Name, string Slug, string? Description);

public record GetCategoriesQuery : IRequest<List<CategoryDto>>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAppCache _cache;

    public GetCategoriesQueryHandler(IApplicationDbContext context, IAppCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _cache.GetOrCreateAsync(
            "cp:categories",
            TimeSpan.FromHours(1),
            async ct => await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto(c.Id, c.Name, c.Slug, c.Description))
                .ToListAsync(ct),
            cancellationToken: cancellationToken);
    }
}