using System.Text.Json.Serialization;

namespace erp.minepress.agentic.ai.Models;

public class ToolDefinitionsFile
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("system")]
    public string System { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("modules")]
    public List<ModuleDefinition> Modules { get; set; } = [];
}

public class ModuleDefinition
{
    [JsonPropertyName("module")]
    public string Module { get; set; } = string.Empty;

    [JsonPropertyName("agent")]
    public string Agent { get; set; } = string.Empty;

    [JsonPropertyName("tools")]
    public List<ToolDefinition> Tools { get; set; } = [];
}

public class ToolDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("parameters")]
    public ToolParameters? Parameters { get; set; }
}

public class ToolParameters
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "object";

    [JsonPropertyName("properties")]
    public Dictionary<string, ToolProperty> Properties { get; set; } = [];

    [JsonPropertyName("required")]
    public List<string> Required { get; set; } = [];
}

public class ToolProperty
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "string";

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
