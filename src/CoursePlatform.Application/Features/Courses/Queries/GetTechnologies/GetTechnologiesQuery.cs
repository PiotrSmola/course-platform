using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Courses.Queries.GetTechnologies;

public record TechnologyDto(Guid Id, string Name, string Slug, string? Description);

public record GetTechnologiesQuery : IRequest<List<TechnologyDto>>;

public class GetTechnologiesQueryHandler : IRequestHandler<GetTechnologiesQuery, List<TechnologyDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAppCache _cache;

    public GetTechnologiesQueryHandler(IApplicationDbContext context, IAppCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<TechnologyDto>> Handle(GetTechnologiesQuery request, CancellationToken cancellationToken)
    {
        return await _cache.GetOrCreateAsync(
            "cp:technologies",
            TimeSpan.FromHours(1),
            async ct => await _context.Technologies
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .Select(t => new TechnologyDto(t.Id, t.Name, t.Slug, t.Description))
                .ToListAsync(ct),
            cancellationToken: cancellationToken);
    }
}