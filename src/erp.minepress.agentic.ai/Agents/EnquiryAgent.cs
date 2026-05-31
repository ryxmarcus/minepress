using erp.minepress.agentic.ai.Models;
using erp.minepress.bff.service.Interfaces;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Agents;

public class EnquiryAgent : BaseAgent
{
    private readonly IAiDataService _data;

    public EnquiryAgent(ILogger<EnquiryAgent> logger, IAiDataService data) : base(logger)
    {
        _data = data;
    }

    public override string AgentName => "EnquiryAgent";

    public override IReadOnlyList<string> SupportedIntents =>
        ["get_enquiries", "search_enquiry", "get_enquiry_details"];

    public override Task<AgentResult> ExecuteAsync(string tool, Dictionary<string, object?> parameters, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("EnquiryAgent executing tool {Tool}", tool);

        if (ToolMatches(tool, "GetAllEnquiries")) return GetAllEnquiriesAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "SearchEnquiry")) return SearchEnquiryAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetEnquiryDetails")) return GetEnquiryDetailsAsync(parameters, cancellationToken);

        return Task.FromResult(AgentResult.Fail($"Unknown tool: {tool}"));
    }

    private async Task<AgentResult> GetAllEnquiriesAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var status = GetStringParameter(parameters, "status");
        var limit = GetIntParameter(parameters, "limit", 20);
        var enquiries = await _data.GetAllEnquiriesAsync(status, limit, ct);

        var statusInfo = string.IsNullOrEmpty(status) ? "" : $" with status '{status}'";
        return AgentResult.Ok(enquiries, "GetAllEnquiries", $"Found {enquiries.Count} enquiry(ies){statusInfo}");
    }

    private async Task<AgentResult> SearchEnquiryAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var keyword = GetStringParameter(parameters, "keyword") ?? GetStringParameter(parameters, "enquiryNo");
        if (string.IsNullOrEmpty(keyword))
            return AgentResult.Fail("Missing required parameter: keyword or enquiryNo");

        var enquiries = await _data.SearchEnquiriesAsync(keyword, ct);
        return enquiries.Count > 0
            ? AgentResult.Ok(enquiries, "SearchEnquiry", $"Found {enquiries.Count} enquiry(ies) matching '{keyword}'")
            : AgentResult.Fail($"No enquiries found matching '{keyword}'");
    }

    private async Task<AgentResult> GetEnquiryDetailsAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var enquiryId = GetIntParameter(parameters, "enquiryId");
        if (enquiryId <= 0)
            return AgentResult.Fail("Missing required parameter: enquiryId");

        var enquiry = await _data.GetEnquiryByIdAsync(enquiryId, ct);
        return enquiry is not null
            ? AgentResult.Ok(enquiry, "GetEnquiryDetails", $"Enquiry {enquiry.EnquiryNo}: {enquiry.CustomerName} — Status: {enquiry.Status}, Items: {enquiry.ItemCount}")
            : AgentResult.Fail($"Enquiry with ID {enquiryId} not found");
    }
}
