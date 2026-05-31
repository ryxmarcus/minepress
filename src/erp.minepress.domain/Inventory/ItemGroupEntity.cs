using erp.minepress.domain.Common;

namespace erp.minepress.domain.Inventory;

public class ItemGroupEntity : AuditableEntity<long>
{
    public string GroupCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public long? ItemCategoryId { get; set; }
    public string? Description { get; set; }
}
