using erp.minepress.infrastructure.ErrorLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace erp.minepress.web.Filters;

/// <summary>
/// Global MVC exception filter — catches any controller/action exception that escapes
/// an action's own try/catch and logs it to sys_error_log via ISystemErrorLogger.
/// Complements GlobalExceptionLoggingMiddleware (which handles truly unhandled exceptions)
/// and per-controller AuditExceptionAsync calls (which handle caught exceptions).
/// </summary>
public class GlobalExceptionFilter : IAsyncExceptionFilter
{
    private readonly ISystemErrorLogger _errorLogger;
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ISystemErrorLogger errorLogger, ILogger<GlobalExceptionFilter> logger)
    {
        _errorLogger = errorLogger;
        _logger = logger;
    }

    public async Task OnExceptionAsync(ExceptionContext context)
    {
        var ex = context.Exception;

        // Log to sys_error_log
        var errorLogId = await _errorLogger.LogAsync(
            ex,
            context.HttpContext,
            severity: "Error",
            additionalData: $"GlobalExceptionFilter: {context.ActionDescriptor.DisplayName}");

        _logger.LogError(ex, "Unhandled controller exception. ErrorLogId: {ErrorLogId}", errorLogId);

        // Return a consistent JSON error response for API routes
        if (context.HttpContext.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new ObjectResult(new
            {
                success = false,
                message = "An unexpected error occurred.",
                errorLogId
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };

            context.ExceptionHandled = true;
        }

        // For non-API routes, leave ExceptionHandled = false so
        // GlobalExceptionLoggingMiddleware can redirect to the error page.
    }
}
