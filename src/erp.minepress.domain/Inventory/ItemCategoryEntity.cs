using erp.minepress.domain.Common;

namespace erp.minepress.domain.Inventory;

public class ItemCategoryEntity : AuditableEntity<long>
{
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
}
