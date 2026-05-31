using erp.minepress.domain.Common;
using erp.minepress.domain.Enums;

namespace erp.minepress.domain.Job;

public class JobTypeEntity : BaseEntity<int>
{
    public string JobTypeCode { get; set; } = string.Empty;
    public string JobTypeName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDesignRequired { get; set; }
    public bool IsDtpRequired { get; set; }
    public bool IsCtpRequired { get; set; }
    public bool IsPrintingRequired { get; set; }
    public bool IsBindingRequired { get; set; }
    public bool IsFinishingRequired { get; set; }
    public string? PrintingMode { get; set; }
    public bool IsSingleProcess { get; set; }
    public bool IsFullProcess { get; set; }
    public bool IsCustomerMaterial { get; set; }
    public bool IsInHouseMaterial { get; set; } = true;
    public bool IsOutsourceJob { get; set; }
    public bool AllowAdvancePayment { get; set; } = true;
    public bool RequireCostingApproval { get; set; }
    public string? DefaultStartProcessCode { get; set; }
    public string? DefaultEndProcessCode { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
