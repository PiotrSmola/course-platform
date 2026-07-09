using MediatR;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Admin.Search;

namespace CoursePlatform.Application.Features.Admin.Queries.GetReindexJob;

public sealed record GetReindexJobQuery : IRequest<ReindexJobState?>;

public class GetReindexJobQueryHandler : IRequestHandler<GetReindexJobQuery, ReindexJobState?>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IReindexJobService _jobService;

    public GetReindexJobQueryHandler(ICurrentUserService currentUser, IReindexJobService jobService)
    {
        _currentUser = currentUser;
        _jobService = jobService;
    }

    public Task<ReindexJobState?> Handle(GetReindexJobQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAdmin)
        {
            throw new ForbiddenAccessException("Admin access required.");
        }

        return Task.FromResult(_jobService.GetCurrent());
    }
}