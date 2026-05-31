using erp.minepress.domain.Common;

namespace erp.minepress.domain.Process;

public class ProcessEntity : BaseEntity<int>
{
    public string ProcessCode { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long DepartmentId { get; set; }
    public int SequenceNo { get; set; }
    public bool IsMandatory { get; set; } = true;
    public bool IsApprovalRequired { get; set; }
    public bool IsClientApproval { get; set; }
    public string? TemplateCode { get; set; }
    public string? TemplateName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public ICollection<SubProcessEntity> SubProcesses { get; set; } = [];
}
