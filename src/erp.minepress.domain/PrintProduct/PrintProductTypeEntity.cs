using erp.minepress.domain.Common;

namespace erp.minepress.domain.PrintProduct;

public class PrintProductTypeEntity : BaseEntity<int>
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public bool IsCustomSize { get; set; } = true;
    public bool IsBindingRequired { get; set; }
    public bool IsPrintingRequired { get; set; } = true;
    public bool IsFinishingRequired { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public ICollection<ProductPartEntity> Parts { get; set; } = [];
}
