using System.Diagnostics;
using MediatR;

namespace CoursePlatform.Application.Common.Behaviours;

public class TracingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public static readonly ActivitySource ActivitySource = new("CoursePlatform");

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(typeof(TRequest).Name);
        activity?.SetTag("mediatr.request", typeof(TRequest).FullName);

        try
        {
            var response = await next();
            activity?.SetStatus(ActivityStatusCode.Ok);
            return response;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
