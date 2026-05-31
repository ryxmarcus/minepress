using erp.minepress.agentic.ai.Models;
using erp.minepress.bff.service.Interfaces;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Agents;

public class ReportingAgent : BaseAgent
{
    private readonly IAiDataService _data;

    public ReportingAgent(ILogger<ReportingAgent> logger, IAiDataService data) : base(logger)
    {
        _data = data;
    }

    public override string AgentName => "ReportingAgent";

    public override IReadOnlyList<string> SupportedIntents =>
        ["get_reports", "send_documents"];

    public override async Task<AgentResult> ExecuteAsync(string tool, Dictionary<string, object?> parameters, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("ReportingAgent executing tool {Tool}", tool);

        var reportType = GetStringParameter(parameters, "reportType") ?? "summary";
        var fromDate = GetStringParameter(parameters, "fromDate");
        var toDate = GetStringParameter(parameters, "toDate");

        DateOnly? from = null;
        DateOnly? to = null;

        if (DateOnly.TryParse(fromDate, out var fd)) from = fd;
        if (DateOnly.TryParse(toDate, out var td)) to = td;

        var summary = await _data.GetReportSummaryAsync(from, to, cancellationToken);

        var result = new
        {
            reportType,
            summary.FromDate,
            summary.ToDate,
            summary.TotalJobs,
            summary.ActiveJobs,
            summary.CompletedJobs,
            summary.CancelledJobs,
            TotalRevenue = $"₹{summary.TotalRevenue:N2}",
            TotalOutstanding = $"₹{summary.TotalOutstanding:N2}",
            summary.TotalInvoices,
            summary.TotalGatePasses,
            summary.TotalMachineAllocations,
            summary.TotalVendorJobs,
            summary.JobsByStatus
        };

        return AgentResult.Ok(result, tool, $"Report '{reportType}' generated: {summary.TotalJobs} jobs, Revenue ₹{summary.TotalRevenue:N2} ({summary.FromDate} to {summary.ToDate})");
    }
}
