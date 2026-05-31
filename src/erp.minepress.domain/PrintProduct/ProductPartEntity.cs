using erp.minepress.domain.Common;

namespace erp.minepress.domain.PrintProduct;

public class ProductPartEntity : BaseEntity<int>
{
    public string PartCode { get; set; } = string.Empty;
    public string PartName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PrintProductTypeId { get; set; }
    public bool IsPageBased { get; set; } = true;
    public bool IsMultiple { get; set; }
    public int DefaultPages { get; set; }
    public bool RequiresDesign { get; set; } = true;
    public bool RequiresPaper { get; set; } = true;
    public bool RequiresPlate { get; set; } = true;
    public bool RequiresPrinting { get; set; } = true;
    public bool RequiresBinding { get; set; }
    public bool RequiresFinishing { get; set; }
    public int? DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public PrintProductTypeEntity? PrintProductType { get; set; }
}
