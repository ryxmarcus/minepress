using System.Text.Json;
using erp.minepress.agentic.ai.Interfaces;
using erp.minepress.agentic.ai.Models;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Services;

public class ToolDefinitionProvider : IToolDefinitionProvider
{
    private static readonly string FilePath = Path.Combine(AppContext.BaseDirectory, "context-file", "tool-definitions.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ILogger<ToolDefinitionProvider> _logger;
    private readonly object _lock = new();
    private ToolDefinitionsFile _definitions;

    public ToolDefinitionProvider(ILogger<ToolDefinitionProvider> logger)
    {
        _logger = logger;
        _definitions = LoadFromFile();
    }

    public IReadOnlyList<ModuleDefinition> GetModules()
    {
        lock (_lock) { return _definitions.Modules; }
    }

    public IReadOnlyList<ToolDefinition> GetToolsForAgent(string agentName)
    {
        lock (_lock)
        {
            var module = _definitions.Modules
                .FirstOrDefault(m => m.Agent.Equals(agentName, StringComparison.OrdinalIgnoreCase));
            return module?.Tools ?? [];
        }
    }

    public ToolDefinition? GetTool(string toolName)
    {
        lock (_lock)
        {
            return _definitions.Modules
                .SelectMany(m => m.Tools)
                .FirstOrDefault(t => t.Name.Equals(toolName, StringComparison.OrdinalIgnoreCase));
        }
    }

    public string? GetAgentNameForTool(string toolName)
    {
        lock (_lock)
        {
            return _definitions.Modules
                .FirstOrDefault(m => m.Tools.Any(t => t.Name.Equals(toolName, StringComparison.OrdinalIgnoreCase)))
                ?.Agent;
        }
    }

    public void RefreshFromDbContext(ToolDefinitionsFile generatedDefinitions)
    {
        lock (_lock)
        {
            var baseline = LoadFromFile();
            _definitions = MergeDefinitions(baseline, generatedDefinitions);
            _logger.LogInformation(
                "Tool definitions refreshed: {Modules} modules, {Tools} total tools",
                _definitions.Modules.Count,
                _definitions.Modules.Sum(m => m.Tools.Count));
        }
    }

    public async Task SaveToFileAsync(CancellationToken cancellationToken = default)
    {
        ToolDefinitionsFile snapshot;
        lock (_lock) { snapshot = _definitions; }

        var dir = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(snapshot, JsonOptions);
        await File.WriteAllTextAsync(FilePath, json, cancellationToken);
        _logger.LogInformation("Tool definitions saved to {Path}", FilePath);
    }

    private ToolDefinitionsFile LoadFromFile()
    {
        if (!File.Exists(FilePath))
        {
            _logger.LogWarning("Tool definitions file not found at {Path}. Using empty definitions.", FilePath);
            return new ToolDefinitionsFile();
        }

        var json = File.ReadAllText(FilePath);
        var defs = JsonSerializer.Deserialize<ToolDefinitionsFile>(json) ?? new ToolDefinitionsFile();
        _logger.LogInformation("Loaded {Count} module(s) from tool definitions", defs.Modules.Count);
        return defs;
    }

    /// <summary>
    /// Merges baseline (hand-crafted) definitions with auto-generated ones.
    /// Baseline tools take precedence; auto-generated tools for new entities are added.
    /// </summary>
    private static ToolDefinitionsFile MergeDefinitions(ToolDefinitionsFile baseline, ToolDefinitionsFile generated)
    {
        var merged = new ToolDefinitionsFile
        {
            Version = generated.Version ?? baseline.Version ?? "3.0-merged",
            System = baseline.System ?? generated.System ?? "MinePress ERP",
            Description = "Merged tool definitions (hand-crafted baseline + DbContext auto-generated)"
        };

        // Index baseline modules by module name
        var baselineModules = baseline.Modules.ToDictionary(m => m.Module, StringComparer.OrdinalIgnoreCase);

        // Start with all baseline modules
        foreach (var bm in baseline.Modules)
        {
            merged.Modules.Add(new ModuleDefinition
            {
                Module = bm.Module,
                Agent = bm.Agent,
                Tools = [.. bm.Tools]
            });
        }

        // Merge generated modules
        foreach (var gm in generated.Modules)
        {
            var existing = merged.Modules.FirstOrDefault(m => m.Module.Equals(gm.Module, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                // Add only tools not already present in baseline
                var existingToolNames = new HashSet<string>(existing.Tools.Select(t => t.Name), StringComparer.OrdinalIgnoreCase);
                foreach (var tool in gm.Tools.Where(t => !existingToolNames.Contains(t.Name)))
                {
                    existing.Tools.Add(tool);
                }
            }
            else
            {
                // Entirely new module from DbContext scan
                merged.Modules.Add(new ModuleDefinition
                {
                    Module = gm.Module,
                    Agent = gm.Agent,
                    Tools = [.. gm.Tools]
                });
            }
        }

        return merged;
    }
}
