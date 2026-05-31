using System.Reflection;
using erp.minepress.agentic.ai.Interfaces;
using erp.minepress.persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Services;

/// <summary>
/// Cross-module analytics service that uses DbContext metadata and dynamic queries
/// to provide business insights across all ERP entities.
/// </summary>
public class AnalyticsService : IAnalyticsService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IDynamicEntityService _dynamicEntityService;
    private readonly IDbContextIntentGenerator _intentGenerator;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        ApplicationDbContext dbContext,
        IDynamicEntityService dynamicEntityService,
        IDbContextIntentGenerator intentGenerator,
        ILogger<AnalyticsService> logger)
    {
        _dbContext = dbContext;
        _dynamicEntityService = dynamicEntityService;
        _intentGenerator = intentGenerator;
        _logger = logger;
    }

    public async Task<IReadOnlyList<object>> GetTopCustomersAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        var customers = await _dynamicEntityService.GetAllAsync("MstCustomer", limit: 500, cancellationToken);
        if (customers.Count == 0)
            return [];

        // Return top customers sorted by name (or available identifier)
        var results = customers
            .Select(c => ExtractSummary(c, ["CustomerName", "Name", "CompanyName", "PartyName"]))
            .Where(s => s is not null)
            .Take(limit)
            .Cast<object>()
            .ToList();

        _logger.LogDebug("GetTopCustomers: {Count} results", results.Count);
        return results;
    }

    public async Task<IReadOnlyList<object>> GetTopMachinesAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        var machines = await _dynamicEntityService.GetAllAsync("MstMachine", limit: 500, cancellationToken);
        if (machines.Count == 0)
            return [];

        var results = machines
            .Select(m => ExtractSummary(m, ["MachineName", "Name", "MachineCode"]))
            .Where(s => s is not null)
            .Take(limit)
            .Cast<object>()
            .ToList();

        _logger.LogDebug("GetTopMachines: {Count} results", results.Count);
        return results;
    }

    public async Task<object> GetDailySummaryAsync(string entityName, DateTime? date = null, CancellationToken cancellationToken = default)
    {
        var targetDate = date ?? DateTime.Today;
        var totalCount = await _dynamicEntityService.CountAsync(entityName, cancellationToken);

        // Try to find and count records for the target date
        var dateFilters = new Dictionary<string, object?>();
        var entityType = _dynamicEntityService.ResolveEntityType(entityName);

        int dailyCount = 0;
        if (entityType is not null)
        {
            var dateProp = FindDateProperty(entityType);
            if (dateProp is not null)
            {
                // Use filter to approximate daily count
                var allRecent = await _dynamicEntityService.GetAllAsync(entityName, limit: 1000, cancellationToken);
                dailyCount = allRecent.Count(r => MatchesDate(r, dateProp, targetDate));
            }
        }

        return new
        {
            Entity = entityName,
            Date = targetDate.ToString("yyyy-MM-dd"),
            DailyCount = dailyCount,
            TotalCount = totalCount,
            Module = _intentGenerator.ResolveModule(entityName)
        };
    }

    public async Task<object> GetMonthlySummaryAsync(string entityName, int? month = null, int? year = null, CancellationToken cancellationToken = default)
    {
        var targetMonth = month ?? DateTime.Today.Month;
        var targetYear = year ?? DateTime.Today.Year;
        var totalCount = await _dynamicEntityService.CountAsync(entityName, cancellationToken);

        var entityType = _dynamicEntityService.ResolveEntityType(entityName);
        int monthlyCount = 0;

        if (entityType is not null)
        {
            var dateProp = FindDateProperty(entityType);
            if (dateProp is not null)
            {
                var allRecent = await _dynamicEntityService.GetAllAsync(entityName, limit: 5000, cancellationToken);
                monthlyCount = allRecent.Count(r => MatchesMonth(r, dateProp, targetMonth, targetYear));
            }
        }

        return new
        {
            Entity = entityName,
            Month = targetMonth,
            Year = targetYear,
            MonthlyCount = monthlyCount,
            TotalCount = totalCount,
            Module = _intentGenerator.ResolveModule(entityName)
        };
    }

    public async Task<object> GetEntityStatsAsync(string entityName, CancellationToken cancellationToken = default)
    {
        var totalCount = await _dynamicEntityService.CountAsync(entityName, cancellationToken);
        var module = _intentGenerator.ResolveModule(entityName);

        return new
        {
            Entity = entityName,
            Module = module,
            TotalCount = totalCount,
            EntityNames = _dynamicEntityService.GetEntityNames().Count
        };
    }

    public async Task<IReadOnlyList<object>> GetModuleOverviewAsync(CancellationToken cancellationToken = default)
    {
        var entities = _intentGenerator.ScanEntities();
        var moduleGroups = entities.GroupBy(e => e.Module);

        var results = new List<object>();
        foreach (var group in moduleGroups.OrderBy(g => g.Key))
        {
            var entityNames = group.Select(e => e.EntityName).ToList();
            int totalCount = 0;

            foreach (var name in entityNames.Take(5)) // Limit per module to keep it fast
            {
                totalCount += await _dynamicEntityService.CountAsync(name, cancellationToken);
            }

            results.Add(new
            {
                Module = group.Key,
                EntityCount = entityNames.Count,
                SampleEntities = entityNames.Take(5).ToList(),
                TotalRecords = totalCount
            });
        }

        _logger.LogDebug("ModuleOverview: {Count} modules", results.Count);
        return results;
    }

    // ─── Private helpers ─────────────────────────────────────────────

    private static object? ExtractSummary(object entity, string[] nameFields)
    {
        var type = entity.GetType();
        string? name = null;

        foreach (var field in nameFields)
        {
            var prop = type.GetProperty(field, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop is not null)
            {
                name = prop.GetValue(entity)?.ToString();
                if (!string.IsNullOrEmpty(name))
                    break;
            }
        }

        // Get the primary key value
        var idProp = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => p.Name.EndsWith("Id", StringComparison.Ordinal) &&
                                 (p.PropertyType == typeof(int) || p.PropertyType == typeof(long)));

        var id = idProp?.GetValue(entity);

        return new { Id = id, Name = name ?? "Unknown", EntityType = type.Name };
    }

    private static PropertyInfo? FindDateProperty(Type entityType)
    {
        string[] dateFields = ["CreatedOn", "CreatedAt", "JobDate", "InvoiceDate", "TransactionDate",
            "VoucherDate", "IssueDate", "ReceiveDate", "OrderDate", "Date"];

        foreach (var field in dateFields)
        {
            var prop = entityType.GetProperty(field, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop is not null && (prop.PropertyType == typeof(DateTime) ||
                                     prop.PropertyType == typeof(DateTime?) ||
                                     prop.PropertyType == typeof(DateOnly) ||
                                     prop.PropertyType == typeof(DateOnly?)))
            {
                return prop;
            }
        }

        return null;
    }

    private static bool MatchesDate(object entity, PropertyInfo dateProp, DateTime targetDate)
    {
        var value = dateProp.GetValue(entity);
        return value switch
        {
            DateTime dt => dt.Date == targetDate.Date,
            DateOnly d => d == DateOnly.FromDateTime(targetDate),
            _ => false
        };
    }

    private static bool MatchesMonth(object entity, PropertyInfo dateProp, int month, int year)
    {
        var value = dateProp.GetValue(entity);
        return value switch
        {
            DateTime dt => dt.Month == month && dt.Year == year,
            DateOnly d => d.Month == month && d.Year == year,
            _ => false
        };
    }
}
