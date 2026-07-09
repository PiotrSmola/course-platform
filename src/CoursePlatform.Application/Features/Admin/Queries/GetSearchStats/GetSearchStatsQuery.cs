using MediatR;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Admin.Search;

namespace CoursePlatform.Application.Features.Admin.Queries.GetSearchStats;

public sealed record GetSearchStatsQuery : IRequest<CourseIndexStats>;

public class GetSearchStatsQueryHandler : IRequestHandler<GetSearchStatsQuery, CourseIndexStats>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICourseIndexingService _indexing;

    public GetSearchStatsQueryHandler(ICurrentUserService currentUser, ICourseIndexingService indexing)
    {
        _currentUser = currentUser;
        _indexing = indexing;
    }

    public async Task<CourseIndexStats> Handle(GetSearchStatsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAdmin)
        {
            throw new ForbiddenAccessException("Admin access required.");
        }

        return await _indexing.GetStatsAsync(cancellationToken);
    }
}