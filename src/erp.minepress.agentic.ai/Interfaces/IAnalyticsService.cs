namespace erp.minepress.agentic.ai.Interfaces;

/// <summary>
/// Cross-module analytics service for the AI ERP.
/// Provides high-level business insights, summaries, and report data.
/// </summary>
public interface IAnalyticsService
{
    /// <summary>Get top customers by total job value.</summary>
    Task<IReadOnlyList<object>> GetTopCustomersAsync(int limit = 10, CancellationToken cancellationToken = default);

    /// <summary>Get top machines by utilization (total jobs assigned).</summary>
    Task<IReadOnlyList<object>> GetTopMachinesAsync(int limit = 10, CancellationToken cancellationToken = default);

    /// <summary>Get daily summary for a specific entity (e.g., jobs, invoices created today).</summary>
    Task<object> GetDailySummaryAsync(string entityName, DateTime? date = null, CancellationToken cancellationToken = default);

    /// <summary>Get monthly summary for a specific entity.</summary>
    Task<object> GetMonthlySummaryAsync(string entityName, int? month = null, int? year = null, CancellationToken cancellationToken = default);

    /// <summary>Get entity-level statistics: total count, recent count, and date range.</summary>
    Task<object> GetEntityStatsAsync(string entityName, CancellationToken cancellationToken = default);

    /// <summary>Get module-level overview with entity counts and recent activity.</summary>
    Task<IReadOnlyList<object>> GetModuleOverviewAsync(CancellationToken cancellationToken = default);
}
