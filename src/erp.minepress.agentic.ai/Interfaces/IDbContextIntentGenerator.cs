using erp.minepress.agentic.ai.Models;

namespace erp.minepress.agentic.ai.Interfaces;

/// <summary>
/// Core automation engine: scans DbContext, generates intent catalogs,
/// tool definitions, and service mappings from database schema.
/// </summary>
public interface IDbContextIntentGenerator
{
    /// <summary>
    /// Scans the DbContext and returns metadata for all entity types.
    /// </summary>
    IReadOnlyList<EntityMetadata> ScanEntities();

    /// <summary>
    /// Generates the full intent catalog from discovered entities.
    /// Includes CRUD, Search, Report, Analytics, and Relationship intents.
    /// </summary>
    IntentCatalog GenerateIntentCatalog();

    /// <summary>
    /// Generates OpenAI-compatible tool definitions from the intent catalog.
    /// </summary>
    ToolDefinitionsFile GenerateToolDefinitions();

    /// <summary>
    /// Gets the generated tool definitions JSON string.
    /// </summary>
    string GenerateToolDefinitionsJson();

    /// <summary>
    /// Returns the module name for a given entity, using naming convention analysis.
    /// </summary>
    string ResolveModule(string entityName);
}
