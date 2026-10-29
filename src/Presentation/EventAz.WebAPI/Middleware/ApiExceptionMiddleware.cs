using EventAz.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace EventAz.WebAPI.Middleware;

public sealed class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
        => (_next, _logger) = (next, logger);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
                throw;

            if (context.RequestAborted.IsCancellationRequested)
                return;

            var (status, title) = GetStatus(exception);
            if (status >= StatusCodes.Status500InternalServerError)
                _logger.LogError(exception, "Unhandled API error. TraceId: {TraceId}", context.TraceIdentifier);
            else
                _logger.LogWarning(exception, "API request failed with status {StatusCode}.", status);

            context.Response.Clear();
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status >= StatusCodes.Status500InternalServerError
                    ? "An unexpected error occurred. Use the traceId when contacting support."
                    : exception.Message,
                Instance = context.Request.Path
            };
            problem.Extensions["traceId"] = context.TraceIdentifier;

            if (exception is ValidationException validationException)
            {
                problem.Extensions["errors"] = validationException.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).Distinct().ToArray());
            }

            await context.Response.WriteAsJsonAsync(problem, cancellationToken: context.RequestAborted);
        }
    }

    private static (int Status, string Title) GetStatus(Exception exception) => exception switch
    {
        ValidationException => (StatusCodes.Status400BadRequest, "Request validation failed"),
        NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
        UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Access denied"),
        ApplicationException => (StatusCodes.Status400BadRequest, "Request could not be processed"),
        ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
        InvalidOperationException => (StatusCodes.Status409Conflict, "Request conflicts with the current state"),
        _ => (StatusCodes.Status500InternalServerError, "Internal server error")
    };
}
