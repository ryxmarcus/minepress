using erp.minepress.agentic.ai.Models;

namespace erp.minepress.agentic.ai.Interfaces;

public interface IToolDefinitionProvider
{
    IReadOnlyList<ModuleDefinition> GetModules();
    IReadOnlyList<ToolDefinition> GetToolsForAgent(string agentName);
    ToolDefinition? GetTool(string toolName);
    string? GetAgentNameForTool(string toolName);

    /// <summary>
    /// Refreshes tool definitions from DbContext scan.
    /// Merges auto-generated definitions with the static JSON baseline.
    /// </summary>
    void RefreshFromDbContext(ToolDefinitionsFile generatedDefinitions);

    /// <summary>
    /// Persists current tool definitions to the JSON file on disk.
    /// </summary>
    Task SaveToFileAsync(CancellationToken cancellationToken = default);
}
