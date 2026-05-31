using Microsoft.AspNetCore.Http;

namespace erp.minepress.infrastructure.ErrorLogging;

public interface ISystemErrorLogger
{
    Task<long?> LogAsync(
        Exception exception,
        HttpContext httpContext,
        string? severity = null,
        string? additionalData = null,
        CancellationToken cancellationToken = default);
}
