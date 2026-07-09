using MediatR;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Admin.Search;

namespace CoursePlatform.Application.Features.Admin.Commands.StartReindex;

public sealed record StartReindexResult(ReindexJobState State, bool AlreadyRunning);

public sealed record StartReindexCommand : IRequest<StartReindexResult>;

public class StartReindexCommandHandler : IRequestHandler<StartReindexCommand, StartReindexResult>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IReindexJobService _jobService;

    public StartReindexCommandHandler(ICurrentUserService currentUser, IReindexJobService jobService)
    {
        _currentUser = currentUser;
        _jobService = jobService;
    }

    public async Task<StartReindexResult> Handle(StartReindexCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAdmin)
        {
            throw new ForbiddenAccessException("Admin access required.");
        }

        var result = await _jobService.StartAsync(cancellationToken);
        return new StartReindexResult(result.State, result.AlreadyRunning);
    }
}