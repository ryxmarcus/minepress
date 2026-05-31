using System.Linq.Expressions;
using System.Reflection;
using erp.minepress.agentic.ai.Interfaces;
using erp.minepress.persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Services;

/// <summary>
/// Generic CRUD service that can operate on any entity registered in ApplicationDbContext.
/// Uses EF Core metadata and reflection to dynamically query entities by name.
/// </summary>
public class DynamicEntityService : IDynamicEntityService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DynamicEntityService> _logger;
    private readonly Dictionary<string, Type> _entityTypeMap;

    public DynamicEntityService(ApplicationDbContext dbContext, ILogger<DynamicEntityService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _entityTypeMap = BuildEntityTypeMap();
    }

    public async Task<IReadOnlyList<object>> GetAllAsync(string entityName, int limit = 50, CancellationToken cancellationToken = default)
    {
        var entityType = ResolveEntityType(entityName);
        if (entityType is null)
            return [];

        var queryable = GetDbSet(entityType);
        if (queryable is null)
            return [];

        var results = await queryable
            .Take(limit)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("GetAll {Entity}: {Count} records returned", entityName, results.Count);
        return results;
    }

    public async Task<object?> GetByIdAsync(string entityName, object id, CancellationToken cancellationToken = default)
    {
        var entityType = ResolveEntityType(entityName);
        if (entityType is null)
            return null;

        var keyValue = ConvertKeyValue(entityType, id);
        if (keyValue is null)
            return null;

        var result = await _dbContext.FindAsync(entityType, [keyValue], cancellationToken);
        _logger.LogDebug("GetById {Entity} ({Id}): {Found}", entityName, id, result is not null ? "found" : "not found");
        return result;
    }

    public async Task<IReadOnlyList<object>> SearchAsync(string entityName, string keyword, int limit = 20, CancellationToken cancellationToken = default)
    {
        var entityType = ResolveEntityType(entityName);
        if (entityType is null || string.IsNullOrWhiteSpace(keyword))
            return [];

        var queryable = GetDbSet(entityType);
        if (queryable is null)
            return [];

        // Build dynamic predicate: any string property Contains(keyword)
        var parameter = Expression.Parameter(entityType, "e");
        Expression? combinedPredicate = null;
        var containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
        var toLowerMethod = typeof(string).GetMethod(nameof(string.ToLowerInvariant), Type.EmptyTypes)!;
        var keywordLower = keyword.ToLowerInvariant();

        var stringProperties = GetStringProperties(entityType);
        foreach (var prop in stringProperties)
        {
            // e.PropertyName != null && e.PropertyName.ToLowerInvariant().Contains(keywordLower)
            var propertyAccess = Expression.Property(parameter, prop);
            var notNull = Expression.NotEqual(propertyAccess, Expression.Constant(null, typeof(string)));
            var toLower = Expression.Call(propertyAccess, toLowerMethod);
            var containsCall = Expression.Call(toLower, containsMethod, Expression.Constant(keywordLower));
            var predicate = Expression.AndAlso(notNull, containsCall);

            combinedPredicate = combinedPredicate is null
                ? predicate
                : Expression.OrElse(combinedPredicate, predicate);
        }

        if (combinedPredicate is null)
            return [];

        var lambda = Expression.Lambda(combinedPredicate, parameter);
        var whereMethod = GetWhereMethod().MakeGenericMethod(entityType);
        var filtered = (IQueryable)whereMethod.Invoke(null, [queryable, lambda])!;

        var results = await filtered
            .Cast<object>()
            .Take(limit)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Search {Entity} for '{Keyword}': {Count} results", entityName, keyword, results.Count);
        return results;
    }

    public async Task<IReadOnlyList<object>> FilterAsync(string entityName, Dictionary<string, object?> filters, int limit = 50, CancellationToken cancellationToken = default)
    {
        var entityType = ResolveEntityType(entityName);
        if (entityType is null || filters.Count == 0)
            return [];

        var queryable = GetDbSet(entityType);
        if (queryable is null)
            return [];

        var parameter = Expression.Parameter(entityType, "e");
        Expression? combinedPredicate = null;

        foreach (var (key, value) in filters)
        {
            if (value is null) continue;

            var prop = entityType.GetProperty(key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop is null) continue;

            var convertedValue = ConvertFilterValue(value, prop.PropertyType);
            if (convertedValue is null) continue;

            var propertyAccess = Expression.Property(parameter, prop);
            var constant = Expression.Constant(convertedValue, prop.PropertyType);
            var equality = Expression.Equal(propertyAccess, constant);

            combinedPredicate = combinedPredicate is null
                ? equality
                : Expression.AndAlso(combinedPredicate, equality);
        }

        if (combinedPredicate is null)
            return [];

        var lambda = Expression.Lambda(combinedPredicate, parameter);
        var whereMethod = GetWhereMethod().MakeGenericMethod(entityType);
        var filtered = (IQueryable)whereMethod.Invoke(null, [queryable, lambda])!;

        var results = await filtered
            .Cast<object>()
            .Take(limit)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Filter {Entity}: {Count} results for {FilterCount} filters", entityName, results.Count, filters.Count);
        return results;
    }

    public async Task<int> CountAsync(string entityName, CancellationToken cancellationToken = default)
    {
        var entityType = ResolveEntityType(entityName);
        if (entityType is null)
            return 0;

        var queryable = GetDbSet(entityType);
        if (queryable is null)
            return 0;

        var count = await queryable.CountAsync(cancellationToken);
        _logger.LogDebug("Count {Entity}: {Count}", entityName, count);
        return count;
    }

    public async Task<bool> ExistsAsync(string entityName, object id, CancellationToken cancellationToken = default)
    {
        var result = await GetByIdAsync(entityName, id, cancellationToken);
        return result is not null;
    }

    public Type? ResolveEntityType(string entityName)
    {
        if (_entityTypeMap.TryGetValue(entityName, out var type))
            return type;

        // Try case-insensitive fallback
        var match = _entityTypeMap.FirstOrDefault(
            kvp => kvp.Key.Equals(entityName, StringComparison.OrdinalIgnoreCase));

        return match.Value;
    }

    public IReadOnlyList<string> GetEntityNames() => _entityTypeMap.Keys.ToList().AsReadOnly();

    // ─── Private helpers ─────────────────────────────────────────────

    private Dictionary<string, Type> BuildEntityTypeMap()
    {
        var map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        foreach (var entityType in _dbContext.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (!clrType.Name.StartsWith("Vw", StringComparison.Ordinal))
            {
                map[clrType.Name] = clrType;
            }
        }
        return map;
    }

    private IQueryable<object>? GetDbSet(Type entityType)
    {
        // Use DbContext.Set(Type) to get a non-generic IQueryable
        var setMethod = typeof(DbContext)
            .GetMethods()
            .First(m => m.Name == nameof(DbContext.Set) && m.IsGenericMethodDefinition && m.GetParameters().Length == 0)
            .MakeGenericMethod(entityType);

        var dbSet = setMethod.Invoke(_dbContext, null);
        return (dbSet as IQueryable)?.Cast<object>();
    }

    private object? ConvertKeyValue(Type entityType, object id)
    {
        var pk = _dbContext.Model.FindEntityType(entityType)?.FindPrimaryKey();
        if (pk is null) return null;

        var pkProperty = pk.Properties.FirstOrDefault();
        if (pkProperty is null) return null;

        return ConvertFilterValue(id, pkProperty.ClrType);
    }

    private static object? ConvertFilterValue(object value, Type targetType)
    {
        try
        {
            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (value is System.Text.Json.JsonElement jsonElement)
            {
                return underlying switch
                {
                    _ when underlying == typeof(int) => jsonElement.GetInt32(),
                    _ when underlying == typeof(long) => jsonElement.GetInt64(),
                    _ when underlying == typeof(decimal) => jsonElement.GetDecimal(),
                    _ when underlying == typeof(double) => jsonElement.GetDouble(),
                    _ when underlying == typeof(bool) => jsonElement.GetBoolean(),
                    _ when underlying == typeof(DateTime) => jsonElement.GetDateTime(),
                    _ when underlying == typeof(string) => jsonElement.GetString(),
                    _ when underlying == typeof(Guid) => jsonElement.GetGuid(),
                    _ => jsonElement.GetString()
                };
            }

            if (underlying == typeof(string))
                return value.ToString();

            return Convert.ChangeType(value, underlying);
        }
        catch
        {
            return null;
        }
    }

    private static List<PropertyInfo> GetStringProperties(Type entityType)
    {
        return entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType == typeof(string) && p.CanRead)
            .Where(p => !p.Name.EndsWith("Json", StringComparison.Ordinal) &&
                        !p.Name.EndsWith("Hash", StringComparison.Ordinal))
            .Take(10) // Limit to avoid overly broad searches
            .ToList();
    }

    private static MethodInfo GetWhereMethod()
    {
        return typeof(Queryable)
            .GetMethods()
            .First(m => m.Name == nameof(Queryable.Where) &&
                        m.GetParameters().Length == 2 &&
                        m.GetParameters()[1].ParameterType.GetGenericArguments()[0]
                            .GetGenericArguments().Length == 2);
    }
}
