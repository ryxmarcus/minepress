namespace erp.minepress.agentic.ai.Models;

/// <summary>
/// Auto-generated intent catalog produced by DbContextIntentGenerator.
/// Maps every discovered entity to its CRUD, search, report, and analytics intents.
/// </summary>
public class IntentCatalog
{
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public int TotalEntities { get; set; }
    public int TotalIntents { get; set; }
    public List<EntityIntentGroup> EntityIntents { get; set; } = [];
}

public class EntityIntentGroup
{
    public string EntityName { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public List<GeneratedIntent> Intents { get; set; } = [];
}

public class GeneratedIntent
{
    public string IntentName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // CRUD, Search, Report, Analytics, Relationship
    public string Description { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public List<string> RequiredParameters { get; set; } = [];
    public List<string> OptionalParameters { get; set; } = [];
}
