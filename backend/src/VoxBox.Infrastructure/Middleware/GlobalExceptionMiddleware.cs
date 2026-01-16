using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog;

namespace VoxBox.Infrastructure.Middleware;

/// <summary>
/// Middleware for handling unhandled exceptions globally across the application.
/// Logs exceptions with Serilog and returns appropriate HTTP responses.
/// </summary>
public class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Log the exception with Serilog for detailed logging
        logger.LogError(exception, "Unhandled exception occurred. Path: {Path}, Method: {Method}, QueryString: {QueryString}, UserAgent: {UserAgent}, RemoteIp: {RemoteIp}", 
            context.Request.Path,
            context.Request.Method,
            context.Request.QueryString.ToString(),
            context.Request.Headers.UserAgent.ToString(),
            context.Connection.RemoteIpAddress);

        // Log with Serilog directly for richer context
        Log.Error(exception, "Global exception handler caught: {ExceptionType} - {ExceptionMessage}",
            exception.GetType().Name,
            exception.Message);

        // Set response status code and content type
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        // Create error response
        var errorResponse = new
        {
            error = new
            {
                message = "An unexpected error occurred. Please try again later.",
                type = exception.GetType().Name,
                statusCode = context.Response.StatusCode,
                // Include stack trace only in Development environment
#if DEBUG
                stackTrace = exception.StackTrace,
                details = exception.Message
#endif
            }
        };

        // Write error response
        await context.Response.WriteAsJsonAsync(errorResponse);
    }
}

/// <summary>
/// Extension methods to add global exception middleware
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
