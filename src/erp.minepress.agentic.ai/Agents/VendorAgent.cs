using erp.minepress.agentic.ai.Models;
using erp.minepress.bff.service.Interfaces;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Agents;

public class VendorAgent : BaseAgent
{
    private readonly IAiDataService _data;

    public VendorAgent(ILogger<VendorAgent> logger, IAiDataService data) : base(logger)
    {
        _data = data;
    }

    public override string AgentName => "VendorAgent";

    public override IReadOnlyList<string> SupportedIntents =>
        ["create_vendor_job", "get_vendors", "search_vendor", "get_outsources_by_job"];

    public override Task<AgentResult> ExecuteAsync(string tool, Dictionary<string, object?> parameters, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("VendorAgent executing tool {Tool}", tool);

        if (ToolMatches(tool, "GetAllVendors")) return GetAllVendorsAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "SearchVendor")) return SearchVendorAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetOutsourcesByJob")) return GetOutsourcesByJobAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "CreateVendorJob")) return CreateVendorJobAsync(parameters, cancellationToken);

        return Task.FromResult(AgentResult.Fail($"Unknown tool: {tool}"));
    }

    private async Task<AgentResult> CreateVendorJobAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var jobId = GetStringParameter(parameters, "jobId");
        var vendorName = GetStringParameter(parameters, "vendorName");
        var processType = GetStringParameter(parameters, "processType");
        var quantity = GetIntParameter(parameters, "quantity");

        if (string.IsNullOrEmpty(jobId))
        {
            return AgentResult.Fail("Missing required parameter: jobId");
        }

        // Check existing outsource jobs
        var existing = await _data.GetOutsourcesByJobNoAsync(jobId, ct);
        if (existing.Count > 0)
        {
            return AgentResult.Ok(existing, "CreateVendorJob",
                $"Job {jobId} already has {existing.Count} vendor outsource(s). Here are the details:");
        }

        // Use vendorId = 1 as default if not provided (first vendor)
        var vendorId = GetIntParameter(parameters, "vendorId");

        var outsource = await _data.CreateVendorJobAsync(
            jobId,
            vendorId > 0 ? vendorId : 1,
            processType,
            quantity > 0 ? quantity : null,
            ct);

        if (outsource is null)
            return AgentResult.Fail($"Failed to create vendor job. Job '{jobId}' not found.");

        Logger.LogInformation("Created vendor job {OsNo} for job {JobId}", outsource.OutsourceNo, jobId);
        return AgentResult.Ok(outsource, "CreateVendorJob", $"Vendor job {outsource.OutsourceNo} created for job {jobId}");
    }

    private async Task<AgentResult> GetAllVendorsAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var limit = GetIntParameter(parameters, "limit", 50);
        var vendors = await _data.GetAllVendorsAsync(limit, ct);
        return AgentResult.Ok(vendors, "GetAllVendors", $"Found {vendors.Count} vendor(s)");
    }

    private async Task<AgentResult> SearchVendorAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var keyword = GetStringParameter(parameters, "keyword") ?? GetStringParameter(parameters, "vendorName");
        if (string.IsNullOrEmpty(keyword))
            return AgentResult.Fail("Missing required parameter: keyword or vendorName");

        var vendors = await _data.SearchVendorsAsync(keyword, ct);
        return vendors.Count > 0
            ? AgentResult.Ok(vendors, "SearchVendor", $"Found {vendors.Count} vendor(s) matching '{keyword}'")
            : AgentResult.Fail($"No vendors found matching '{keyword}'");
    }

    private async Task<AgentResult> GetOutsourcesByJobAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var jobId = GetStringParameter(parameters, "jobId");
        if (string.IsNullOrEmpty(jobId))
            return AgentResult.Fail("Missing required parameter: jobId");

        var outsources = await _data.GetOutsourcesByJobNoAsync(jobId, ct);
        return outsources.Count > 0
            ? AgentResult.Ok(outsources, "GetOutsourcesByJob", $"Found {outsources.Count} outsource(s) for job {jobId}")
            : AgentResult.Fail($"No outsources found for job {jobId}");
    }
}
