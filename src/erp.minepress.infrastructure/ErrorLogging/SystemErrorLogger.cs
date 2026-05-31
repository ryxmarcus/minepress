using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using erp.minepress.persistence.Context;
using erp.minepress.persistence.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace erp.minepress.infrastructure.ErrorLogging;

public class SystemErrorLogger : ISystemErrorLogger
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SystemErrorLogger> _logger;

    public SystemErrorLogger(IServiceScopeFactory scopeFactory, ILogger<SystemErrorLogger> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<long?> LogAsync(
        Exception exception,
        HttpContext httpContext,
        string? severity = null,
        string? additionalData = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = httpContext.Request;
            var endpoint = httpContext.GetEndpoint();
            var routeValues = httpContext.GetRouteData()?.Values;

            // Resolve user — try claims first (works for JWT/cookie auth), then session key
            var userId = 0;
            var userName = "SYSTEM";

            var userIdClaim = httpContext.User?.FindFirst(ClaimTypes.NameIdentifier)
                              ?? httpContext.User?.FindFirst("userId");
            var userNameClaim = httpContext.User?.FindFirst(ClaimTypes.Name)
                                ?? httpContext.User?.FindFirst("userName");

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var claimsUserId))
                userId = claimsUserId;
            if (!string.IsNullOrWhiteSpace(userNameClaim?.Value))
                userName = userNameClaim.Value;

            var requestData = await BuildRequestDataAsync(httpContext, cancellationToken);
            var source = endpoint?.DisplayName;
            string? methodName = null;

            if (routeValues != null)
            {
                if (string.IsNullOrWhiteSpace(source) && routeValues.TryGetValue("controller", out var controllerValue))
                    source = controllerValue?.ToString();

                if (routeValues.TryGetValue("action", out var actionValue))
                    methodName = actionValue?.ToString();
            }

            source ??= "Unknown";

            var layer = request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
                ? "API"
                : "UI";

            var entry = new SysErrorLog
            {
                Layer = layer,
                Source = Limit(source, 500),
                MethodName = string.IsNullOrWhiteSpace(methodName) ? null : Limit(methodName, 500),
                ExceptionType = Limit(exception.GetType().FullName ?? exception.GetType().Name, 500),
                Message = exception.Message,
                StackTrace = exception.StackTrace ?? exception.ToString(),
                InnerException = exception.InnerException?.ToString() ?? string.Empty,
                RequestPath = Limit(request.Path.HasValue ? request.Path.Value! : string.Empty, 500),
                HttpMethod = Limit(request.Method, 10),
                RequestData = requestData,
                UserId = userId,
                UserName = Limit(userName, 100),
                IpAddress = Limit(httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty, 50),
                UserAgent = Limit(request.Headers.UserAgent.ToString(), 500),
                CorrelationId = Limit(httpContext.TraceIdentifier, 50),
                TenantKey = Limit(ResolveTenantKey(request), 50),
                Severity = Limit(string.IsNullOrWhiteSpace(severity) ? "Error" : severity, 20),
                AdditionalData = additionalData ?? string.Empty,
                CreatedOn = DateTime.UtcNow,
                MachineName = Limit(Environment.MachineName, 100),
                AppVersion = Limit(Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0", 50),
                IsReviewed = false,
                ReviewNotes = string.Empty,
                ReviewedBy = string.Empty,
                ReviewedOn = new DateTime(1900, 1, 1)
            };

            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.SysErrorLogs.Add(entry);
            await db.SaveChangesAsync(cancellationToken);

            return entry.ErrorLogId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist exception into sys_error_log.");
            return null;
        }
    }

    private static async Task<string> BuildRequestDataAsync(HttpContext context, CancellationToken cancellationToken)
    {
        var request = context.Request;

        var data = new
        {
            query = request.Query.ToDictionary(q => q.Key, q => q.Value.ToString()),
            route = context.GetRouteData()?.Values.ToDictionary(v => v.Key, v => v.Value?.ToString() ?? string.Empty),
            body = await ReadRequestBodyAsync(request, cancellationToken)
        };

        return JsonSerializer.Serialize(data);
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        if (HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method))
            return string.Empty;

        if (request.ContentLength is null or <= 0)
            return string.Empty;

        request.EnableBuffering();
        request.Body.Position = 0;

        using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        var body = await reader.ReadToEndAsync(cancellationToken);
        request.Body.Position = 0;

        return body.Length > 10000 ? body[..10000] : body;
    }

    private static string ResolveTenantKey(HttpRequest request)
    {
        if (request.Headers.TryGetValue("X-Tenant-Key", out var tenantHeader) && !string.IsNullOrWhiteSpace(tenantHeader))
            return tenantHeader.ToString();

        return "DEFAULT";
    }

    private static string Limit(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value.Length > maxLength ? value[..maxLength] : value;
    }
}
