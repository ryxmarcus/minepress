using System.Text.Json;
using erp.minepress.infrastructure.ErrorLogging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace erp.minepress.infrastructure.Middleware;

public class GlobalExceptionLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionLoggingMiddleware> _logger;

    public GlobalExceptionLoggingMiddleware(RequestDelegate next, ILogger<GlobalExceptionLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ISystemErrorLogger systemErrorLogger)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var errorLogId = await systemErrorLogger.LogAsync(
                ex,
                context,
                severity: "Critical",
                additionalData: "GlobalExceptionLoggingMiddleware");

            _logger.LogError(ex, "Unhandled exception captured. ErrorLogId: {ErrorLogId}", errorLogId);

            if (context.Response.HasStarted)
            {
                throw;
            }

            if (context.Request.Path.StartsWithSegments("/Error", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync($"An unexpected error occurred. Reference ID: {errorLogId}");
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "An unexpected error occurred.",
                    errorLogId
                }));
                return;
            }

            var query = new Dictionary<string, string?>
            {
                ["errorId"] = errorLogId?.ToString(),
                ["message"] = "An unexpected error occurred while processing your request."
            };

            var errorUrl = QueryHelpers.AddQueryString("/Error", query);
            context.Response.Redirect(errorUrl);
        }
    }
}
