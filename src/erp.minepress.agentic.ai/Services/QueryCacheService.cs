using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Services;

/// <summary>
/// Memory cache wrapper for frequent AI queries.
/// Reduces database load for repeated identical queries within a short window.
/// </summary>
public class QueryCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<QueryCacheService> _logger;
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

    public QueryCacheService(IMemoryCache cache, ILogger<QueryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Get or create a cached result for the given cache key.
    /// If not found, executes the factory and caches the result.
    /// </summary>
    public async Task<T> GetOrCreateAsync<T>(string cacheKey, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (_cache.TryGetValue(cacheKey, out T? cached) && cached is not null)
        {
            _logger.LogDebug("Cache HIT: {Key}", cacheKey);
            return cached;
        }

        _logger.LogDebug("Cache MISS: {Key}", cacheKey);
        var result = await factory();

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration,
            SlidingExpiration = TimeSpan.FromMinutes(2)
        };

        _cache.Set(cacheKey, result, options);
        return result;
    }

    /// <summary>Build a cache key for entity queries.</summary>
    public static string BuildKey(string operation, string entityName, params string[] extras)
    {
        var key = $"ai:{operation}:{entityName}";
        if (extras.Length > 0)
            key += ":" + string.Join(":", extras);
        return key.ToLowerInvariant();
    }

    /// <summary>Invalidate all cache entries for a specific entity.</summary>
    public void InvalidateEntity(string entityName)
    {
        // MemoryCache doesn't support prefix-based invalidation natively,
        // so we track and remove known keys per entity.
        // For simplicity, we rely on expiration for now.
        _logger.LogDebug("Cache invalidation requested for entity: {Entity}", entityName);
    }

    /// <summary>Remove a specific cache entry.</summary>
    public void Remove(string cacheKey)
    {
        _cache.Remove(cacheKey);
        _logger.LogDebug("Cache entry removed: {Key}", cacheKey);
    }
}
