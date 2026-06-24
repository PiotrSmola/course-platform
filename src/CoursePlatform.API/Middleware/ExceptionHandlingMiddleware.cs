using System.Net;
using System.Text.Json;
using CoursePlatform.Application.Common.Exceptions;
using FluentValidation;

namespace CoursePlatform.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error");
            var errors = string.Join(", ", ex.Errors.Select(e => e.ErrorMessage));
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var result = JsonSerializer.Serialize(new { error = message, statusCode = (int)statusCode });
        return context.Response.WriteAsync(result);
    }
}
