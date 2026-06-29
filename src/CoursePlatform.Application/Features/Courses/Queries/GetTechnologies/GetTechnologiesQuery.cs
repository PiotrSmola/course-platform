using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Application.Features.Courses.Queries.GetTechnologies;

public record TechnologyDto(Guid Id, string Name, string Slug, string? Description);

public record GetTechnologiesQuery : IRequest<List<TechnologyDto>>;

public class GetTechnologiesQueryHandler : IRequestHandler<GetTechnologiesQuery, List<TechnologyDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTechnologiesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TechnologyDto>> Handle(GetTechnologiesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Technologies
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new TechnologyDto(t.Id, t.Name, t.Slug, t.Description))
            .ToListAsync(cancellationToken);
    }
}