using erp.minepress.agentic.ai.Models;
using erp.minepress.bff.service.Interfaces;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Agents;

public class QuotationAgent : BaseAgent
{
    private readonly IAiDataService _data;

    public QuotationAgent(ILogger<QuotationAgent> logger, IAiDataService data) : base(logger)
    {
        _data = data;
    }

    public override string AgentName => "QuotationAgent";

    public override IReadOnlyList<string> SupportedIntents =>
        ["get_quotations", "search_quotation", "get_quotation_details"];

    public override Task<AgentResult> ExecuteAsync(string tool, Dictionary<string, object?> parameters, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("QuotationAgent executing tool {Tool}", tool);

        if (ToolMatches(tool, "GetAllQuotations")) return GetAllQuotationsAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "SearchQuotation")) return SearchQuotationAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetQuotationDetails")) return GetQuotationDetailsAsync(parameters, cancellationToken);

        return Task.FromResult(AgentResult.Fail($"Unknown tool: {tool}"));
    }

    private async Task<AgentResult> GetAllQuotationsAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var status = GetStringParameter(parameters, "status");
        var limit = GetIntParameter(parameters, "limit", 20);
        var quotations = await _data.GetAllQuotationsAsync(status, limit, ct);

        var statusInfo = string.IsNullOrEmpty(status) ? "" : $" with status '{status}'";
        return AgentResult.Ok(quotations, "GetAllQuotations", $"Found {quotations.Count} quotation(s){statusInfo}");
    }

    private async Task<AgentResult> SearchQuotationAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var keyword = GetStringParameter(parameters, "keyword") ?? GetStringParameter(parameters, "quotationNo");
        if (string.IsNullOrEmpty(keyword))
            return AgentResult.Fail("Missing required parameter: keyword or quotationNo");

        var quotations = await _data.SearchQuotationsAsync(keyword, ct);
        return quotations.Count > 0
            ? AgentResult.Ok(quotations, "SearchQuotation", $"Found {quotations.Count} quotation(s) matching '{keyword}'")
            : AgentResult.Fail($"No quotations found matching '{keyword}'");
    }

    private async Task<AgentResult> GetQuotationDetailsAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var quotationId = GetIntParameter(parameters, "quotationId");
        if (quotationId <= 0)
            return AgentResult.Fail("Missing required parameter: quotationId");

        var quotation = await _data.GetQuotationByIdAsync(quotationId, ct);
        return quotation is not null
            ? AgentResult.Ok(quotation, "GetQuotationDetails", $"Quotation {quotation.QuotationNo}: {quotation.CustomerName} — Amount: ₹{quotation.NetAmount:N2}, Status: {quotation.Status}")
            : AgentResult.Fail($"Quotation with ID {quotationId} not found");
    }
}
