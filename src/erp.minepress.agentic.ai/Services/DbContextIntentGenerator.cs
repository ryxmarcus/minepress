using System.Text.Json;
using System.Text.RegularExpressions;
using erp.minepress.agentic.ai.Interfaces;
using erp.minepress.agentic.ai.Models;
using erp.minepress.persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace erp.minepress.agentic.ai.Services;

/// <summary>
/// Core automation engine: scans ApplicationDbContext, generates services,
/// creates tool-definitions.json, and builds intent catalog.
/// This is the automation engine of the AI ERP.
/// </summary>
public class DbContextIntentGenerator : IDbContextIntentGenerator
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DbContextIntentGenerator> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // Module mapping: entity name prefix → module name
    private static readonly Dictionary<string, string> ModulePrefixMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["MstCustomer"] = "Customer",
        ["MstParty"] = "Customer",
        ["TrnJob"] = "Job",
        ["MstJob"] = "Job",
        ["HybJobRate"] = "Job",
        ["MstMachine"] = "Machine",
        ["TrnMachine"] = "Machine",
        ["MstEmployeeMachine"] = "Machine",
        ["TrnSalesInvoice"] = "Billing",
        ["TrnCreditNote"] = "Billing",
        ["TrnDebitNote"] = "Billing",
        ["TrnGatePass"] = "Delivery",
        ["TrnChallan"] = "Delivery",
        ["MstVendor"] = "Vendor",
        ["TrnOutsource"] = "Vendor",
        ["TrnJobOutsource"] = "Vendor",
        ["Hr"] = "HR",
        ["Hyb"] = "HR",
        ["TrnEnquiry"] = "Enquiry",
        ["TrnQuotation"] = "Quotation",
        ["TrnPurchase"] = "Purchase",
        ["TrnGoodsReceipt"] = "Purchase",
        ["TrnReceipt"] = "Accounting",
        ["TrnPayment"] = "Accounting",
        ["TrnBankReceipt"] = "Accounting",
        ["TrnBankPayment"] = "Accounting",
        ["TrnJournalVoucher"] = "Accounting",
        ["TrnContraVoucher"] = "Accounting",
        ["TrnExpenseVoucher"] = "Accounting",
        ["TrnLedger"] = "Accounting",
        ["TrnAccountLedger"] = "Accounting",
        ["TrnAdvanceLedger"] = "Accounting",
        ["TrnTaxLedger"] = "Accounting",
        ["TrnTdsLedger"] = "Accounting",
        ["TrnApOutstanding"] = "Accounting",
        ["TrnArOutstanding"] = "Accounting",
        ["TrnBankReconciliation"] = "Accounting",
        ["TrnProformaInvoice"] = "Accounting",
        ["TrnStoreIssue"] = "Store",
        ["TrnStoreReceive"] = "Store",
        ["TrnStore"] = "Store",
        ["TrnStockLedger"] = "Store",
        ["MstPaper"] = "Store",
        ["MstInk"] = "Store",
        ["MstPlate"] = "Store",
        ["MstBinding"] = "Store",
        ["MstFinishing"] = "Store",
        ["MstChemical"] = "Store",
        ["MstMaterial"] = "Store",
        ["MstItem"] = "Store",
        ["MstOtherItem"] = "Store",
        ["TrnNotification"] = "Notification",
        ["TrnUserNotification"] = "Notification",
        ["TrnAiAgent"] = "AI",
        ["TrnAiNotification"] = "AI",
        ["MstNotification"] = "Notification",
        ["TxnNotification"] = "Notification",
        ["MstProcess"] = "Production",
        ["MstSubProcess"] = "Production",
        ["MstPrintProcess"] = "Production",
        ["MstProcessStage"] = "Production",
        ["MstProcessNotification"] = "Production",
    };

    // Module → Agent name mapping
    private static readonly Dictionary<string, string> ModuleAgentMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Job"] = "JobAgent",
        ["Customer"] = "CustomerAgent",
        ["Machine"] = "MachineAgent",
        ["Billing"] = "BillingAgent",
        ["Delivery"] = "DeliveryAgent",
        ["Vendor"] = "VendorAgent",
        ["HR"] = "HRAgent",
        ["Enquiry"] = "EnquiryAgent",
        ["Quotation"] = "QuotationAgent",
        ["Purchase"] = "PurchaseAgent",
        ["Accounting"] = "AccountingAgent",
        ["Store"] = "StoreAgent",
        ["Reporting"] = "ReportingAgent",
        ["Production"] = "ProductionAgent",
        ["Notification"] = "NotificationAgent",
        ["AI"] = "SystemAgent",
        ["Master"] = "MasterDataAgent",
    };

    public DbContextIntentGenerator(ApplicationDbContext dbContext, ILogger<DbContextIntentGenerator> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public IReadOnlyList<EntityMetadata> ScanEntities()
    {
        var entities = new List<EntityMetadata>();
        var model = _dbContext.Model;

        foreach (var entityType in model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            // Skip views
            if (clrType.Name.StartsWith("Vw", StringComparison.Ordinal))
                continue;

            var tableName = entityType.GetTableName() ?? clrType.Name;
            var schema = entityType.GetSchema() ?? "public";

            var metadata = new EntityMetadata
            {
                EntityName = clrType.Name,
                ClrTypeName = clrType.FullName ?? clrType.Name,
                TableName = tableName,
                SchemaName = schema,
                Module = ResolveModule(clrType.Name),
                DbSetName = FindDbSetName(clrType.Name)
            };

            // Properties
            foreach (var prop in entityType.GetProperties())
            {
                metadata.Properties.Add(new EntityPropertyMetadata
                {
                    Name = prop.Name,
                    ClrType = prop.ClrType.Name,
                    IsNullable = prop.IsNullable,
                    IsPrimaryKey = prop.IsPrimaryKey(),
                    IsForeignKey = prop.IsForeignKey(),
                    MaxLength = prop.GetMaxLength()
                });
            }

            // Primary keys
            var pk = entityType.FindPrimaryKey();
            if (pk is not null)
            {
                metadata.PrimaryKeyProperties = pk.Properties.Select(p => p.Name).ToList();
            }

            // Relationships
            foreach (var nav in entityType.GetNavigations())
            {
                var targetType = nav.TargetEntityType.ClrType.Name;
                var isCollection = nav.IsCollection;

                metadata.Relationships.Add(new EntityRelationshipMetadata
                {
                    RelatedEntityName = targetType,
                    RelationshipType = isCollection ? "OneToMany" : "ManyToOne",
                    NavigationProperty = nav.Name,
                    ForeignKeyProperty = nav.ForeignKey?.Properties.FirstOrDefault()?.Name
                });
            }

            entities.Add(metadata);
        }

        _logger.LogInformation("DbContext scan complete: {Count} entities discovered", entities.Count);
        return entities;
    }

    public IntentCatalog GenerateIntentCatalog()
    {
        var entities = ScanEntities();
        var catalog = new IntentCatalog
        {
            TotalEntities = entities.Count
        };

        foreach (var entity in entities)
        {
            var group = new EntityIntentGroup
            {
                EntityName = entity.EntityName,
                Module = entity.Module,
                AgentName = ResolveAgent(entity.Module)
            };

            var friendlyName = GetFriendlyName(entity.EntityName);
            var pkProp = entity.PrimaryKeyProperties.FirstOrDefault() ?? "id";

            // CRUD intents
            group.Intents.Add(new GeneratedIntent
            {
                IntentName = $"create_{friendlyName}",
                Category = "CRUD",
                Description = $"Create a new {friendlyName}",
                ToolName = $"Create{ToPascalCase(friendlyName)}",
                RequiredParameters = GetWritableProperties(entity),
                OptionalParameters = []
            });

            group.Intents.Add(new GeneratedIntent
            {
                IntentName = $"update_{friendlyName}",
                Category = "CRUD",
                Description = $"Update an existing {friendlyName}",
                ToolName = $"Update{ToPascalCase(friendlyName)}",
                RequiredParameters = [pkProp],
                OptionalParameters = GetWritableProperties(entity)
            });

            group.Intents.Add(new GeneratedIntent
            {
                IntentName = $"delete_{friendlyName}",
                Category = "CRUD",
                Description = $"Delete a {friendlyName}",
                ToolName = $"Delete{ToPascalCase(friendlyName)}",
                RequiredParameters = [pkProp],
                OptionalParameters = []
            });

            group.Intents.Add(new GeneratedIntent
            {
                IntentName = $"get_{friendlyName}_by_id",
                Category = "CRUD",
                Description = $"Get {friendlyName} by ID",
                ToolName = $"Get{ToPascalCase(friendlyName)}ById",
                RequiredParameters = [pkProp],
                OptionalParameters = []
            });

            group.Intents.Add(new GeneratedIntent
            {
                IntentName = $"get_all_{friendlyName}s",
                Category = "CRUD",
                Description = $"Get all {friendlyName} records",
                ToolName = $"GetAll{ToPascalCase(friendlyName)}s",
                RequiredParameters = [],
                OptionalParameters = ["limit", "status"]
            });

            // Search intent
            group.Intents.Add(new GeneratedIntent
            {
                IntentName = $"search_{friendlyName}",
                Category = "Search",
                Description = $"Search {friendlyName} records by keyword",
                ToolName = $"Search{ToPascalCase(friendlyName)}",
                RequiredParameters = ["keyword"],
                OptionalParameters = ["limit"]
            });

            // Filter intent
            group.Intents.Add(new GeneratedIntent
            {
                IntentName = $"filter_{friendlyName}",
                Category = "Search",
                Description = $"Filter {friendlyName} records by field values",
                ToolName = $"Filter{ToPascalCase(friendlyName)}",
                RequiredParameters = [],
                OptionalParameters = GetFilterableProperties(entity)
            });

            // Count intent
            group.Intents.Add(new GeneratedIntent
            {
                IntentName = $"count_{friendlyName}",
                Category = "Analytics",
                Description = $"Count total {friendlyName} records",
                ToolName = $"Count{ToPascalCase(friendlyName)}",
                RequiredParameters = [],
                OptionalParameters = ["status"]
            });

            // Exists intent
            group.Intents.Add(new GeneratedIntent
            {
                IntentName = $"exists_{friendlyName}",
                Category = "Analytics",
                Description = $"Check if {friendlyName} exists",
                ToolName = $"Exists{ToPascalCase(friendlyName)}",
                RequiredParameters = [pkProp],
                OptionalParameters = []
            });

            // Relationship intents
            foreach (var rel in entity.Relationships.Where(r => r.RelationshipType == "OneToMany"))
            {
                var relName = GetFriendlyName(rel.RelatedEntityName);
                group.Intents.Add(new GeneratedIntent
                {
                    IntentName = $"get_{friendlyName}_{relName}s",
                    Category = "Relationship",
                    Description = $"Get {relName} records for a {friendlyName}",
                    ToolName = $"Get{ToPascalCase(friendlyName)}{ToPascalCase(relName)}s",
                    RequiredParameters = [pkProp],
                    OptionalParameters = ["limit"]
                });
            }

            // Report intents for transaction entities
            if (entity.EntityName.StartsWith("Trn", StringComparison.Ordinal))
            {
                group.Intents.Add(new GeneratedIntent
                {
                    IntentName = $"get_daily_{friendlyName}_report",
                    Category = "Report",
                    Description = $"Get daily report for {friendlyName}",
                    ToolName = $"GetDaily{ToPascalCase(friendlyName)}Report",
                    RequiredParameters = [],
                    OptionalParameters = ["date", "fromDate", "toDate"]
                });

                group.Intents.Add(new GeneratedIntent
                {
                    IntentName = $"get_monthly_{friendlyName}_report",
                    Category = "Report",
                    Description = $"Get monthly report for {friendlyName}",
                    ToolName = $"GetMonthly{ToPascalCase(friendlyName)}Report",
                    RequiredParameters = [],
                    OptionalParameters = ["month", "year"]
                });
            }

            catalog.EntityIntents.Add(group);
        }

        catalog.TotalIntents = catalog.EntityIntents.Sum(g => g.Intents.Count);
        _logger.LogInformation("Intent catalog generated: {Entities} entities, {Intents} intents",
            catalog.TotalEntities, catalog.TotalIntents);

        return catalog;
    }

    public ToolDefinitionsFile GenerateToolDefinitions()
    {
        var catalog = GenerateIntentCatalog();

        var toolDefs = new ToolDefinitionsFile
        {
            Version = "3.0-auto",
            System = "MinePress ERP",
            Description = "Auto-generated tool definitions from DbContext schema scan"
        };

        // Group by module
        var moduleGroups = catalog.EntityIntents
            .GroupBy(e => e.Module)
            .OrderBy(g => g.Key);

        foreach (var moduleGroup in moduleGroups)
        {
            var moduleDef = new ModuleDefinition
            {
                Module = moduleGroup.Key,
                Agent = moduleGroup.First().AgentName
            };

            foreach (var entityGroup in moduleGroup)
            {
                foreach (var intent in entityGroup.Intents)
                {
                    var tool = new ToolDefinition
                    {
                        Name = intent.ToolName,
                        Description = intent.Description,
                        Parameters = new ToolParameters
                        {
                            Type = "object",
                            Required = intent.RequiredParameters
                        }
                    };

                    foreach (var param in intent.RequiredParameters.Concat(intent.OptionalParameters))
                    {
                        tool.Parameters.Properties[param] = new ToolProperty
                        {
                            Type = InferParameterType(param),
                            Description = FormatParameterDescription(param)
                        };
                    }

                    moduleDef.Tools.Add(tool);
                }
            }

            toolDefs.Modules.Add(moduleDef);
        }

        _logger.LogInformation("Generated {Count} module tool definitions", toolDefs.Modules.Count);
        return toolDefs;
    }

    public string GenerateToolDefinitionsJson()
    {
        var definitions = GenerateToolDefinitions();
        return JsonSerializer.Serialize(definitions, JsonOptions);
    }

    public string ResolveModule(string entityName)
    {
        // Check exact prefix matches (longest first)
        foreach (var kvp in ModulePrefixMap.OrderByDescending(k => k.Key.Length))
        {
            if (entityName.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        }

        // Fallback: use prefix convention
        if (entityName.StartsWith("Mst", StringComparison.Ordinal))
            return "Master";
        if (entityName.StartsWith("Trn", StringComparison.Ordinal))
            return "Transaction";
        if (entityName.StartsWith("Hr", StringComparison.Ordinal))
            return "HR";
        if (entityName.StartsWith("Hyb", StringComparison.Ordinal))
            return "Hybrid";
        if (entityName.StartsWith("Sys", StringComparison.Ordinal))
            return "System";
        if (entityName.StartsWith("Txn", StringComparison.Ordinal))
            return "Transaction";

        return "General";
    }

    private static string ResolveAgent(string module)
    {
        return ModuleAgentMap.TryGetValue(module, out var agent)
            ? agent
            : $"{module}Agent";
    }

    private string FindDbSetName(string entityName)
    {
        // Use reflection on the DbContext type to find the DbSet property
        var contextType = _dbContext.GetType();
        var dbSetProps = contextType.GetProperties()
            .Where(p => p.PropertyType.IsGenericType &&
                        p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

        foreach (var prop in dbSetProps)
        {
            var genericArg = prop.PropertyType.GetGenericArguments().FirstOrDefault();
            if (genericArg?.Name == entityName)
                return prop.Name;
        }

        return entityName + "s";
    }

    private static string GetFriendlyName(string entityName)
    {
        // Remove prefixes: Mst, Trn, Hr, Hyb, Sys, Txn
        var name = entityName;
        string[] prefixes = ["Mst", "Trn", "Hyb", "Sys", "Txn"];
        foreach (var prefix in prefixes)
        {
            if (name.StartsWith(prefix, StringComparison.Ordinal) && name.Length > prefix.Length)
            {
                name = name[prefix.Length..];
                break;
            }
        }

        // If starts with "Hr" and next char is uppercase, strip Hr
        if (name.StartsWith("Hr", StringComparison.Ordinal) && name.Length > 2 && char.IsUpper(name[2]))
        {
            name = name[2..];
        }

        // Convert PascalCase to snake_case
        return Regex.Replace(name, "(?<!^)([A-Z])", "_$1").ToLowerInvariant();
    }

    private static string ToPascalCase(string snakeCase)
    {
        return string.Concat(snakeCase.Split('_')
            .Select(s => s.Length > 0 ? char.ToUpperInvariant(s[0]) + s[1..] : s));
    }

    private static List<string> GetWritableProperties(EntityMetadata entity)
    {
        return entity.Properties
            .Where(p => !p.IsPrimaryKey && !p.Name.EndsWith("Id", StringComparison.Ordinal) || p.IsForeignKey)
            .Where(p => p.Name is not ("CreatedOn" or "ModifiedOn" or "CreatedBy" or "ModifiedBy"
                         or "CreatedAt" or "UpdatedAt" or "IsDeleted" or "RowVersion"))
            .Select(p => ToCamelCase(p.Name))
            .Take(10) // Limit for practical tool definition size
            .ToList();
    }

    private static List<string> GetFilterableProperties(EntityMetadata entity)
    {
        return entity.Properties
            .Where(p => !p.IsPrimaryKey)
            .Where(p => p.ClrType is "String" or "Int32" or "Int64" or "Boolean" or "DateOnly" or "DateTime")
            .Select(p => ToCamelCase(p.Name))
            .Take(8)
            .ToList();
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    private static string InferParameterType(string paramName)
    {
        if (paramName.EndsWith("Id", StringComparison.OrdinalIgnoreCase) ||
            paramName is "limit" or "quantity" or "year" or "month" or "count")
            return "integer";

        if (paramName is "amount" or "rate" or "total" or "price" or "cost" or "balance")
            return "number";

        if (paramName.Contains("date", StringComparison.OrdinalIgnoreCase) ||
            paramName.Contains("Date", StringComparison.Ordinal))
            return "string";

        if (paramName.Contains("is", StringComparison.OrdinalIgnoreCase) && paramName.Length > 2 &&
            char.IsUpper(paramName[2]))
            return "boolean";

        return "string";
    }

    private static string FormatParameterDescription(string paramName)
    {
        // Convert camelCase to human-readable
        var spaced = Regex.Replace(paramName, "(?<!^)([A-Z])", " $1");
        return char.ToUpperInvariant(spaced[0]) + spaced[1..].ToLowerInvariant();
    }
}
