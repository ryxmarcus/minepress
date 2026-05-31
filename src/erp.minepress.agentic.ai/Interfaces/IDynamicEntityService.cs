namespace erp.minepress.agentic.ai.Interfaces;

/// <summary>
/// Generic CRUD service that operates on any DbContext entity dynamically.
/// Enables the auto-generated intents from DbContextIntentGenerator to execute against the database.
/// </summary>
public interface IDynamicEntityService
{
    /// <summary>Get all records of the specified entity type with optional limit.</summary>
    Task<IReadOnlyList<object>> GetAllAsync(string entityName, int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>Get a single entity by its primary key value.</summary>
    Task<object?> GetByIdAsync(string entityName, object id, CancellationToken cancellationToken = default);

    /// <summary>Search entities by keyword across string properties.</summary>
    Task<IReadOnlyList<object>> SearchAsync(string entityName, string keyword, int limit = 20, CancellationToken cancellationToken = default);

    /// <summary>Filter entities by a dictionary of property name → value pairs.</summary>
    Task<IReadOnlyList<object>> FilterAsync(string entityName, Dictionary<string, object?> filters, int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>Count total records for the specified entity.</summary>
    Task<int> CountAsync(string entityName, CancellationToken cancellationToken = default);

    /// <summary>Check if an entity with the given primary key exists.</summary>
    Task<bool> ExistsAsync(string entityName, object id, CancellationToken cancellationToken = default);

    /// <summary>Get the CLR type for a registered entity name.</summary>
    Type? ResolveEntityType(string entityName);

    /// <summary>Get all known entity names.</summary>
    IReadOnlyList<string> GetEntityNames();
}
