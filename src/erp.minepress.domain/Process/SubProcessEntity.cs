using erp.minepress.domain.Common;

namespace erp.minepress.domain.Process;

public class SubProcessEntity : BaseEntity<int>
{
    public int ProcessId { get; set; }
    public string SubProcessCode { get; set; } = string.Empty;
    public string SubProcessName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long DepartmentId { get; set; }
    public int SequenceNo { get; set; }
    public int? ApprovalTypeId { get; set; }
    public int ApprovalLevel { get; set; }
    public bool IsClientApproval { get; set; }
    public bool IsMandatory { get; set; } = true;
    public bool IsMandatoryApproval { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public ProcessEntity? Process { get; set; }
}
