using System.Text.Json;
using System.Text.RegularExpressions;
using erp.minepress.agentic.ai.Interfaces;
using erp.minepress.agentic.ai.Models;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Agents;

public abstract class BaseAgent : IAgent
{
    protected readonly ILogger Logger;

    protected BaseAgent(ILogger logger)
    {
        Logger = logger;
    }

    public abstract string AgentName { get; }
    public abstract IReadOnlyList<string> SupportedIntents { get; }

    public abstract Task<AgentResult> ExecuteAsync(string tool, Dictionary<string, object?> parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Normalizes a tool/intent name to a canonical lowercase form for matching.
    /// "GetTopCustomers", "get_top_customers", "gettop_customers" → "gettopcustomers"
    /// </summary>
    protected static string NormalizeTool(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;
        return Regex.Replace(name, @"[\s_\-]", "").ToLowerInvariant();
    }

    /// <summary>
    /// Match incoming tool name against a known tool name, case and format agnostic.
    /// </summary>
    protected static bool ToolMatches(string incomingTool, string knownTool)
    {
        return NormalizeTool(incomingTool) == NormalizeTool(knownTool);
    }

    protected T? GetParameter<T>(Dictionary<string, object?> parameters, string key)
    {
        if (!parameters.TryGetValue(key, out var value) || value is null)
            return default;

        if (value is T typed)
            return typed;

        if (value is JsonElement element)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(element.GetRawText());
            }
            catch
            {
                return default;
            }
        }

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return default;
        }
    }

    protected string? GetStringParameter(Dictionary<string, object?> parameters, string key)
    {
        if (!parameters.TryGetValue(key, out var value) || value is null)
            return null;

        if (value is JsonElement element)
            return element.GetString();

        return value.ToString();
    }

    protected int GetIntParameter(Dictionary<string, object?> parameters, string key, int defaultValue = 0)
    {
        if (!parameters.TryGetValue(key, out var value) || value is null)
            return defaultValue;

        if (value is JsonElement element && element.TryGetInt32(out var intVal))
            return intVal;

        if (int.TryParse(value.ToString(), out var parsed))
            return parsed;

        return defaultValue;
    }
}
