using System.Net;
using System.Text.Json;
using CoursePlatform.Application.Common.Exceptions;
using FluentValidation;

namespace CoursePlatform.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");
            await HandleExceptionAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (ForbiddenAccessException ex)
        {
            _logger.LogWarning(ex, "Forbidden access");
            await HandleExceptionAsync(context, HttpStatusCode.Forbidden, ex.Message);
        }
        catch (InvalidWebhookSignatureException ex)
        {
            _logger.LogWarning(ex, "Invalid webhook signature");
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error");
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToList());
            await HandleValidationExceptionAsync(context, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleUnhandledExceptionAsync(context, _environment, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var result = JsonSerializer.Serialize(new { error = message, statusCode = (int)statusCode });
        return context.Response.WriteAsync(result);
    }

    private static Task HandleUnhandledExceptionAsync(
        HttpContext context,
        IWebHostEnvironment environment,
        Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var traceId = context.TraceIdentifier;

        if (environment.IsDevelopment())
        {
            var result = JsonSerializer.Serialize(new
            {
                error = "UnhandledException",
                message = exception.Message,
                exception = exception.GetType().FullName,
                stackTrace = exception.ToString(),
                traceId,
                statusCode = (int)HttpStatusCode.InternalServerError
            });

            return context.Response.WriteAsync(result);
        }

        var generic = JsonSerializer.Serialize(new
        {
            error = "An unexpected error occurred.",
            traceId,
            statusCode = (int)HttpStatusCode.InternalServerError
        });

        return context.Response.WriteAsync(generic);
    }

    private static Task HandleValidationExceptionAsync(HttpContext context, Dictionary<string, List<string>> errors)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var result = JsonSerializer.Serialize(new { errors, statusCode = (int)HttpStatusCode.BadRequest });
        return context.Response.WriteAsync(result);
    }
}
