using erp.minepress.agentic.ai.Interfaces;
using erp.minepress.agentic.ai.Models;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Agents;

/// <summary>
/// Agent for cross-module analytics and reporting.
/// Handles top-customers, top-machines, daily/monthly reports, module overviews.
/// </summary>
public class AnalyticsAgent : BaseAgent
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsAgent(IAnalyticsService analyticsService, ILogger<AnalyticsAgent> logger) : base(logger)
    {
        _analyticsService = analyticsService;
    }

    public override string AgentName => "AnalyticsAgent";

    public override IReadOnlyList<string> SupportedIntents =>
    [
        "get_top_customers",
        "get_top_machines",
        "get_daily_summary",
        "get_monthly_summary",
        "get_entity_stats",
        "get_module_overview"
    ];

    public override async Task<AgentResult> ExecuteAsync(string tool, Dictionary<string, object?> parameters, CancellationToken cancellationToken = default)
    {
        try
        {
            if (ToolMatches(tool, "GetTopCustomers"))
                return AgentResult.Ok(
                    await _analyticsService.GetTopCustomersAsync(
                        GetIntParameter(parameters, "limit", 10), cancellationToken), tool);

            if (ToolMatches(tool, "GetTopMachines"))
                return AgentResult.Ok(
                    await _analyticsService.GetTopMachinesAsync(
                        GetIntParameter(parameters, "limit", 10), cancellationToken), tool);

            if (ToolMatches(tool, "GetDailySummary"))
                return AgentResult.Ok(
                    await _analyticsService.GetDailySummaryAsync(
                        GetStringParameter(parameters, "entityName") ?? "TrnJob",
                        ParseDate(GetStringParameter(parameters, "date")),
                        cancellationToken), tool);

            if (ToolMatches(tool, "GetMonthlySummary"))
                return AgentResult.Ok(
                    await _analyticsService.GetMonthlySummaryAsync(
                        GetStringParameter(parameters, "entityName") ?? "TrnJob",
                        GetIntParameter(parameters, "month", 0) is > 0 and var m ? m : null,
                        GetIntParameter(parameters, "year", 0) is > 0 and var y ? y : null,
                        cancellationToken), tool);

            if (ToolMatches(tool, "GetEntityStats"))
                return AgentResult.Ok(
                    await _analyticsService.GetEntityStatsAsync(
                        GetStringParameter(parameters, "entityName") ?? "TrnJob",
                        cancellationToken), tool);

            if (ToolMatches(tool, "GetModuleOverview"))
                return AgentResult.Ok(
                    await _analyticsService.GetModuleOverviewAsync(cancellationToken), tool);

            return AgentResult.Fail($"Unknown analytics tool: {tool}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "AnalyticsAgent error executing {Tool}", tool);
            return AgentResult.Fail($"Analytics error: {ex.Message}");
        }
    }

    private static DateTime? ParseDate(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr)) return null;
        return DateTime.TryParse(dateStr, out var dt) ? dt : null;
    }
}
