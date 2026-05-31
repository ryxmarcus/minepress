using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Individual steps within a workflow template. Each step has routing, assignment, notification, and visual position metadata.
/// </summary>
public partial class MstWorkflowStep
{
    public long WorkflowStepId { get; set; }

    public long WorkflowTemplateId { get; set; }

    public int? ProcessId { get; set; }

    public int? SubProcessId { get; set; }

    public string StepCode { get; set; } = null!;

    public string StepName { get; set; } = null!;

    public string StepType { get; set; } = null!;

    public int SequenceNo { get; set; }

    public long? DepartmentId { get; set; }

    public long? AssignedUserId { get; set; }

    public string? AssignmentRule { get; set; }

    public int? ApprovalTypeId { get; set; }

    public int? ApprovalLevelId { get; set; }

    public bool IsMandatory { get; set; }

    public decimal? SlaHours { get; set; }

    public decimal? EscalateAfterHours { get; set; }

    public string? EscalateTo { get; set; }

    public bool NotifyVendor { get; set; }

    public bool NotifySupplier { get; set; }

    public bool NotifyCustomer { get; set; }

    public bool NotifyAssignedUser { get; set; }

    public bool NotifyDeptHead { get; set; }

    public bool SendEmail { get; set; }

    public bool SendSms { get; set; }

    public bool SendWhatsapp { get; set; }

    public bool SendPushNotification { get; set; }

    public double CanvasX { get; set; }

    public double CanvasY { get; set; }

    public string? NodeColor { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    /// <summary>
    /// If TRUE, workflow cannot progress until this step is completed. If FALSE (non-blocking), workflow can proceed to next step even if this task is pending. Party-related tasks are typically non-blocking.
    /// </summary>
    public bool IsBlocking { get; set; }

    /// <summary>
    /// If TRUE, this step is included when workflow starts from an enquiry.
    /// </summary>
    public bool AppliesToEnquiry { get; set; }

    /// <summary>
    /// If TRUE, this step is included when workflow starts from a quotation.
    /// </summary>
    public bool AppliesToQuotation { get; set; }

    /// <summary>
    /// If TRUE, this step is included when workflow starts directly from a job.
    /// </summary>
    public bool AppliesToJob { get; set; }

    public virtual MstApprovalLevel? ApprovalLevel { get; set; }

    public virtual MstApprovalType? ApprovalType { get; set; }

    public virtual MstUser? AssignedUser { get; set; }

    public virtual MstDepartment? Department { get; set; }

    public virtual ICollection<MstWorkflowConnection> MstWorkflowConnectionFromSteps { get; set; } = new List<MstWorkflowConnection>();

    public virtual ICollection<MstWorkflowConnection> MstWorkflowConnectionToSteps { get; set; } = new List<MstWorkflowConnection>();

    public virtual MstProcess? Process { get; set; }

    public virtual ICollection<TrnWorkspaceTask> TrnWorkspaceTasks { get; set; } = new List<TrnWorkspaceTask>();

    public virtual MstWorkflowTemplate WorkflowTemplate { get; set; } = null!;
}
