using erp.minepress.agentic.ai.Models;
using erp.minepress.bff.service.Interfaces;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Agents;

public class StoreAgent : BaseAgent
{
    private readonly IAiDataService _data;

    public StoreAgent(ILogger<StoreAgent> logger, IAiDataService data) : base(logger)
    {
        _data = data;
    }

    public override string AgentName => "StoreAgent";

    public override IReadOnlyList<string> SupportedIntents =>
        ["get_store_issues", "get_store_receives", "get_materials", "search_material",
         "get_papers", "get_inks", "get_plates", "get_bindings", "get_finishings"];

    public override Task<AgentResult> ExecuteAsync(string tool, Dictionary<string, object?> parameters, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("StoreAgent executing tool {Tool}", tool);

        if (ToolMatches(tool, "GetStoreIssues")) return GetStoreIssuesAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetStoreReceives")) return GetStoreReceivesAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetAllMaterials")) return GetAllMaterialsAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "SearchMaterial")) return SearchMaterialAsync(parameters, cancellationToken);
        if (ToolMatches(tool, "GetAllPapers")) return GetAllPapersAsync(cancellationToken);
        if (ToolMatches(tool, "GetAllInks")) return GetAllInksAsync(cancellationToken);
        if (ToolMatches(tool, "GetAllPlates")) return GetAllPlatesAsync(cancellationToken);
        if (ToolMatches(tool, "GetAllBindings")) return GetAllBindingsAsync(cancellationToken);
        if (ToolMatches(tool, "GetAllFinishings")) return GetAllFinishingsAsync(cancellationToken);

        return Task.FromResult(AgentResult.Fail($"Unknown tool: {tool}"));
    }

    private async Task<AgentResult> GetStoreIssuesAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var status = GetStringParameter(parameters, "status");
        var limit = GetIntParameter(parameters, "limit", 20);
        var issues = await _data.GetStoreIssuesAsync(status, limit, ct);
        return AgentResult.Ok(issues, "GetStoreIssues", $"Found {issues.Count} store issue(s)");
    }

    private async Task<AgentResult> GetStoreReceivesAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var status = GetStringParameter(parameters, "status");
        var limit = GetIntParameter(parameters, "limit", 20);
        var receives = await _data.GetStoreReceivesAsync(status, limit, ct);
        return AgentResult.Ok(receives, "GetStoreReceives", $"Found {receives.Count} store receive(s)");
    }

    private async Task<AgentResult> GetAllMaterialsAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var category = GetStringParameter(parameters, "category");
        var materials = await _data.GetAllMaterialsAsync(category, ct);
        return AgentResult.Ok(materials, "GetAllMaterials", $"Found {materials.Count} material(s)");
    }

    private async Task<AgentResult> SearchMaterialAsync(Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var keyword = GetStringParameter(parameters, "keyword");
        if (string.IsNullOrEmpty(keyword))
            return AgentResult.Fail("Missing required parameter: keyword");

        var materials = await _data.SearchMaterialsAsync(keyword, ct);
        return materials.Count > 0
            ? AgentResult.Ok(materials, "SearchMaterial", $"Found {materials.Count} material(s) matching '{keyword}'")
            : AgentResult.Fail($"No materials found matching '{keyword}'");
    }

    private async Task<AgentResult> GetAllPapersAsync(CancellationToken ct)
    {
        var papers = await _data.GetAllPapersAsync(ct);
        return AgentResult.Ok(papers, "GetAllPapers", $"Found {papers.Count} paper type(s)");
    }

    private async Task<AgentResult> GetAllInksAsync(CancellationToken ct)
    {
        var inks = await _data.GetAllInksAsync(ct);
        return AgentResult.Ok(inks, "GetAllInks", $"Found {inks.Count} ink type(s)");
    }

    private async Task<AgentResult> GetAllPlatesAsync(CancellationToken ct)
    {
        var plates = await _data.GetAllPlatesAsync(ct);
        return AgentResult.Ok(plates, "GetAllPlates", $"Found {plates.Count} plate type(s)");
    }

    private async Task<AgentResult> GetAllBindingsAsync(CancellationToken ct)
    {
        var bindings = await _data.GetAllBindingsAsync(ct);
        return AgentResult.Ok(bindings, "GetAllBindings", $"Found {bindings.Count} binding type(s)");
    }

    private async Task<AgentResult> GetAllFinishingsAsync(CancellationToken ct)
    {
        var finishings = await _data.GetAllFinishingsAsync(ct);
        return AgentResult.Ok(finishings, "GetAllFinishings", $"Found {finishings.Count} finishing type(s)");
    }
}
