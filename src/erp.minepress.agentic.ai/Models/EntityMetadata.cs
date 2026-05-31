namespace erp.minepress.agentic.ai.Models;

/// <summary>
/// Metadata extracted from DbContext entity types via reflection.
/// Used by DbContextIntentGenerator to auto-generate intents and tool definitions.
/// </summary>
public class EntityMetadata
{
    public string EntityName { get; set; } = string.Empty;
    public string ClrTypeName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string DbSetName { get; set; } = string.Empty;
    public List<EntityPropertyMetadata> Properties { get; set; } = [];
    public List<string> PrimaryKeyProperties { get; set; } = [];
    public List<EntityRelationshipMetadata> Relationships { get; set; } = [];
}

public class EntityPropertyMetadata
{
    public string Name { get; set; } = string.Empty;
    public string ClrType { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsForeignKey { get; set; }
    public int? MaxLength { get; set; }
}

public class EntityRelationshipMetadata
{
    public string RelatedEntityName { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty; // "OneToMany", "ManyToOne", "ManyToMany"
    public string NavigationProperty { get; set; } = string.Empty;
    public string? ForeignKeyProperty { get; set; }
}
