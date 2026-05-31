using System.Text.RegularExpressions;
using erp.minepress.agentic.ai.Interfaces;
using erp.minepress.agentic.ai.Models;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Agents;

/// <summary>
/// Dynamic fallback agent that handles ANY tool/intent by parsing the intent name,
/// inferring the operation and entity, then executing against the database via DynamicEntityService.
/// This is the true agentic layer — nothing is hardcoded.
/// </summary>
public class DynamicFallbackAgent : BaseAgent
{
    private readonly IDynamicEntityService _dynamicService;
    private readonly IDbContextIntentGenerator _intentGenerator;

    public DynamicFallbackAgent(
        IDynamicEntityService dynamicService,
        IDbContextIntentGenerator intentGenerator,
        ILogger<DynamicFallbackAgent> logger) : base(logger)
    {
    _dynamicService = dynamicService;
        _intentGenerator = intentGenerator;
    }

    public override string AgentName => "DynamicFallbackAgent";

    public override IReadOnlyList<string> SupportedIntents => [];

    public override async Task<AgentResult> ExecuteAsync(string tool, Dictionary<string, object?> parameters, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("DynamicFallbackAgent resolving tool '{Tool}' dynamically", tool);

        var normalized = NormalizeTool(tool);

        // Parse operation and entity from the tool/intent name
        var parsed = ParseToolIntent(tool);

        if (parsed.Entity is null)
        {
            Logger.LogWarning("Could not resolve entity from tool '{Tool}'", tool);
            return AgentResult.Fail($"I couldn't determine what data you need from '{tool}'. Could you be more specific?");
        }

        // Try to find the matching entity name in the DbContext
        var entityName = ResolveEntityName(parsed.Entity);

        if (entityName is null)
        {
            Logger.LogWarning("Entity '{Entity}' not found in DbContext for tool '{Tool}'", parsed.Entity, tool);
            return AgentResult.Fail($"Entity '{parsed.Entity}' not found in the database. Available entities can be listed via scan-entities.");
        }

        Logger.LogInformation("Dynamic execution: Operation={Operation}, Entity={Entity}, DbEntity={DbEntity}",
            parsed.Operation, parsed.Entity, entityName);

        return parsed.Operation switch
        {
            "search" => await SearchEntityAsync(entityName, parameters, cancellationToken),
            "get_by_id" or "getbyid" or "details" => await GetByIdAsync(entityName, parameters, cancellationToken),
            "get_all" or "getall" or "list" or "get" => await GetAllAsync(entityName, parameters, cancellationToken),
            "count" => await CountEntityAsync(entityName, cancellationToken),
            "exists" => await ExistsEntityAsync(entityName, parameters, cancellationToken),
            "filter" => await FilterEntityAsync(entityName, parameters, cancellationToken),
            "top" => await GetTopAsync(entityName, parameters, cancellationToken),
            _ => await GetAllAsync(entityName, parameters, cancellationToken)
        };
    }

    private async Task<AgentResult> GetAllAsync(string entityName, Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var limit = GetIntParameter(parameters, "limit", 20);
        var results = await _dynamicService.GetAllAsync(entityName, limit, ct);
        return AgentResult.Ok(results, $"GetAll_{entityName}",
            $"Found {results.Count} {entityName} record(s)");
    }

    private async Task<AgentResult> SearchEntityAsync(string entityName, Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var keyword = GetStringParameter(parameters, "keyword")
                      ?? GetStringParameter(parameters, "name")
                      ?? GetStringParameter(parameters, "query")
                      ?? GetStringParameter(parameters, "search")
                      ?? GetStringParameter(parameters, "customerName")
                      ?? GetStringParameter(parameters, "vendorName")
                      ?? GetStringParameter(parameters, "employeeName");

        // Try any string parameter as keyword
        if (string.IsNullOrEmpty(keyword))
        {
            keyword = parameters.Values
                .Where(v => v is not null)
                .Select(v => v!.ToString())
                .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v) && !int.TryParse(v, out _));
        }

        if (string.IsNullOrEmpty(keyword))
        {
            // Fall back to get all
            return await GetAllAsync(entityName, parameters, ct);
        }

        var limit = GetIntParameter(parameters, "limit", 20);
        var results = await _dynamicService.SearchAsync(entityName, keyword, limit, ct);

        return results.Count > 0
            ? AgentResult.Ok(results, $"Search_{entityName}", $"Found {results.Count} {entityName} record(s) matching '{keyword}'")
            : AgentResult.Fail($"No {entityName} records found matching '{keyword}'");
    }

    private async Task<AgentResult> GetByIdAsync(string entityName, Dictionary<string, object?> parameters, CancellationToken ct)
    {
        // Try common ID parameter names
        object? id = GetStringParameter(parameters, "id")
                     ?? GetStringParameter(parameters, "entityId")
                     ?? (object?)GetIntParameter(parameters, "id")
                     ?? (object?)GetIntParameter(parameters, "entityId");

        // Try any parameter ending with "Id"
        if (id is null or "0" or 0)
        {
            var idParam = parameters.FirstOrDefault(kvp =>
                kvp.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && kvp.Value is not null);
            id = idParam.Value;
        }

        if (id is null)
            return AgentResult.Fail($"Missing ID parameter to look up {entityName}");

        var result = await _dynamicService.GetByIdAsync(entityName, id, ct);
        return result is not null
            ? AgentResult.Ok(result, $"GetById_{entityName}", $"Found {entityName} record")
            : AgentResult.Fail($"{entityName} with the given ID not found");
    }

    private async Task<AgentResult> CountEntityAsync(string entityName, CancellationToken ct)
    {
        var count = await _dynamicService.CountAsync(entityName, ct);
        return AgentResult.Ok(new { entity = entityName, count }, $"Count_{entityName}",
            $"Total {entityName} records: {count}");
    }

    private async Task<AgentResult> ExistsEntityAsync(string entityName, Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var idParam = parameters.FirstOrDefault(kvp =>
            kvp.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && kvp.Value is not null);

        if (idParam.Value is null)
            return AgentResult.Fail($"Missing ID parameter to check existence of {entityName}");

        var exists = await _dynamicService.ExistsAsync(entityName, idParam.Value, ct);
        return AgentResult.Ok(new { entity = entityName, id = idParam.Value, exists }, $"Exists_{entityName}",
            exists ? $"{entityName} exists" : $"{entityName} does not exist");
    }

    private async Task<AgentResult> FilterEntityAsync(string entityName, Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var limit = GetIntParameter(parameters, "limit", 50);
        var filters = parameters
            .Where(kvp => kvp.Key != "limit" && kvp.Value is not null)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        if (filters.Count == 0)
            return await GetAllAsync(entityName, parameters, ct);

        var results = await _dynamicService.FilterAsync(entityName, filters, limit, ct);
        return AgentResult.Ok(results, $"Filter_{entityName}",
            $"Found {results.Count} {entityName} record(s) matching filters");
    }

    private async Task<AgentResult> GetTopAsync(string entityName, Dictionary<string, object?> parameters, CancellationToken ct)
    {
        var limit = GetIntParameter(parameters, "limit", 10);
        var results = await _dynamicService.GetAllAsync(entityName, limit, ct);
        return AgentResult.Ok(results, $"Top_{entityName}",
            $"Top {results.Count} {entityName} record(s)");
    }

    /// <summary>
    /// Parse a tool/intent name into an operation + entity.
    /// Handles formats: "get_top_customers", "GetTopCustomers", "search_customer",
    /// "SearchCustomer", "get_all_jobs", "count_customer" etc.
    /// </summary>
    private (string Operation, string? Entity) ParseToolIntent(string tool)
    {
        // Normalize: convert PascalCase to snake_case first
        var snakeCase = Regex.Replace(tool, "(?<!^)([A-Z])", "_$1").ToLowerInvariant();
        // Clean up double underscores
        snakeCase = Regex.Replace(snakeCase, "_+", "_").Trim('_');
        var parts = snakeCase.Split('_', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
            return ("get", null);

        // Detect operation prefix
        string operation;
        int entityStart;

        if (parts[0] == "get" && parts.Length > 1)
        {
            if (parts[1] is "all" or "top" or "daily" or "monthly" or "yearly")
            {
                operation = parts[1] == "all" ? "get_all" : parts[1];
                entityStart = 2;
            }
            else if (parts.Length > 2 && parts[^1] == "id")
            {
                // get_customer_by_id, get_entity_id
                operation = "get_by_id";
                entityStart = 1;
                parts = parts[..^1]; // remove "id" from entity parsing
                if (parts.Length > 2 && parts[^1] == "by")
                    parts = parts[..^1]; // remove "by"
            }
            else
            {
                operation = "get";
                entityStart = 1;
            }
        }
        else if (parts[0] is "search" or "find" or "lookup")
        {
            operation = "search";
            entityStart = 1;
        }
        else if (parts[0] is "count")
        {
            operation = "count";
            entityStart = 1;
        }
        else if (parts[0] is "exists" or "check")
        {
            operation = "exists";
            entityStart = 1;
        }
        else if (parts[0] is "filter")
        {
            operation = "filter";
            entityStart = 1;
        }
        else if (parts[0] is "create" or "add" or "new")
        {
            operation = "create";
            entityStart = 1;
        }
        else if (parts[0] is "update" or "edit" or "modify")
        {
            operation = "update";
            entityStart = 1;
        }
        else if (parts[0] is "delete" or "remove")
        {
            operation = "delete";
            entityStart = 1;
        }
        else if (parts[0] is "top")
        {
            operation = "top";
            entityStart = 1;
        }
        else
        {
            operation = "get";
            entityStart = 0;
        }

        // Build entity name from remaining parts
        var entityParts = parts.Skip(entityStart).ToArray();
        if (entityParts.Length == 0)
            return (operation, null);

        // Remove trailing 's' for plurals
        var entityRaw = string.Join("_", entityParts);
        var entitySingular = entityRaw.TrimEnd('s');
        if (entitySingular == entityRaw && entityRaw.Length > 1)
            entitySingular = entityRaw; // wasn't plural

        return (operation, entitySingular);
    }

    /// <summary>
    /// Resolve a friendly entity name (from intent) to the actual DbContext entity name.
    /// Maps "customer" → "MstCustomer", "job" → "TrnJob", etc. using DbContext metadata.
    /// </summary>
    private string? ResolveEntityName(string friendlyName)
    {
        var allEntities = _dynamicService.GetEntityNames();
        var normalized = friendlyName.Replace("_", "").ToLowerInvariant();

        // 1. Direct match (case-insensitive)
        var direct = allEntities.FirstOrDefault(e =>
            e.Equals(friendlyName, StringComparison.OrdinalIgnoreCase));
        if (direct is not null) return direct;

        // 2. Match by suffix (e.g., "customer" matches "MstCustomer")
        var suffixMatch = allEntities.FirstOrDefault(e =>
            e.EndsWith(normalized, StringComparison.OrdinalIgnoreCase));
        if (suffixMatch is not null) return suffixMatch;

        // 3. Match by stripped prefix (Mst/Trn/Hr/Hyb removed, then compare)
        var strippedMatch = allEntities.FirstOrDefault(e =>
        {
            var stripped = StripPrefix(e).Replace("_", "").ToLowerInvariant();
            return stripped == normalized || stripped == normalized + "s" || stripped + "s" == normalized;
        });
        if (strippedMatch is not null) return strippedMatch;

        // 4. Contains match as last resort
        var containsMatch = allEntities.FirstOrDefault(e =>
            e.Contains(friendlyName, StringComparison.OrdinalIgnoreCase));

        return containsMatch;
    }

    private static string StripPrefix(string entityName)
    {
        string[] prefixes = ["Mst", "Trn", "Hyb", "Sys", "Txn", "Hr"];
        foreach (var prefix in prefixes)
        {
            if (entityName.StartsWith(prefix, StringComparison.Ordinal) && entityName.Length > prefix.Length)
                return entityName[prefix.Length..];
        }
        return entityName;
    }
}
