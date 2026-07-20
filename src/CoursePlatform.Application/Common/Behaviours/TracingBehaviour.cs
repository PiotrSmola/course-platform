using System.Diagnostics;
using MediatR;
using CoursePlatform.Application.Common.Exceptions;

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
            // Expected client-side failures (validation, not found, forbidden) are not server faults —
            // tagging them Error inflates error rates and leaks input-shaped messages into telemetry.
            if (IsExpected(ex))
            {
                activity?.SetTag("outcome", ex.GetType().Name);
            }
            else
            {
                activity?.SetStatus(ActivityStatusCode.Error);
                activity?.AddException(ex);
            }
            throw;
        }
    }

    private static bool IsExpected(Exception ex) =>
        ex is FluentValidation.ValidationException
            or NotFoundException
            or ForbiddenAccessException;
}
