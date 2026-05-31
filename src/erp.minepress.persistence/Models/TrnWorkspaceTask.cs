using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Consolidated workspace task table aggregating tasks, approvals and follow-ups from all ERP modules. Provides a single source for the My Workspace dashboard. Rows are created by triggers or application logic when tasks/approvals are assigned.
/// </summary>
public partial class TrnWorkspaceTask
{
    public long WorkspaceTaskId { get; set; }

    public long UserId { get; set; }

    /// <summary>
    /// Origin table: trn_job, trn_enquiry, trn_quotation, trn_challan, trn_purchase_order, trn_sales_invoice, etc.
    /// </summary>
    public string SourceTable { get; set; } = null!;

    public long SourceId { get; set; }

    public string? SourceNo { get; set; }

    /// <summary>
    /// TASK: assigned work item, APPROVAL: pending approval request, REVIEW: QC/review item, FOLLOW_UP: CRM/payment follow-up
    /// </summary>
    public string TaskType { get; set; } = null!;

    /// <summary>
    /// PENDING, IN_PROGRESS, COMPLETED, OVERDUE, CANCELLED, REJECTED, APPROVED
    /// </summary>
    public string TaskStatus { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int? ProcessId { get; set; }

    public int? SubprocessId { get; set; }

    public string? ProcessCode { get; set; }

    public string? SubprocessCode { get; set; }

    public long? DepartmentId { get; set; }

    public long? AssignedBy { get; set; }

    public DateTime? AssignedOn { get; set; }

    public string? Priority { get; set; }

    public DateTime? DueDate { get; set; }

    public decimal? SlaHours { get; set; }

    public bool IsOverdue { get; set; }

    public int? ApprovalTypeId { get; set; }

    public int? ApprovalLevel { get; set; }

    public long? CompletedBy { get; set; }

    public DateTime? CompletedOn { get; set; }

    public string? CompletionRemarks { get; set; }

    public string? ActionUrl { get; set; }

    public long? JobId { get; set; }

    public string? JobNo { get; set; }

    public string? PartyName { get; set; }

    public string? Metadata { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public bool IsArchived { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Sequence number within the workflow. Used for ordering tasks in pre-generated workflow.
    /// </summary>
    public int? SequenceNo { get; set; }

    /// <summary>
    /// Reference to the workflow step that generated this task. Null for ad-hoc tasks.
    /// </summary>
    public long? WorkflowStepId { get; set; }

    /// <summary>
    /// Reference to the workflow template. Null for ad-hoc tasks.
    /// </summary>
    public long? WorkflowTemplateId { get; set; }

    /// <summary>
    /// Workflow batch ID to group all tasks belonging to the same workflow instance.
    /// </summary>
    public Guid? WorkflowBatchId { get; set; }

    /// <summary>
    /// If TRUE, this task blocks workflow progression. If FALSE, workflow can proceed to next step even while this task is pending. Inherited from workflow step but can be overridden.
    /// </summary>
    public bool IsBlocking { get; set; }

    public virtual MstUser? AssignedByNavigation { get; set; }

    public virtual MstDepartment? Department { get; set; }

    public virtual MstProcess? Process { get; set; }

    public virtual ICollection<TrnDesignWorkEntry> TrnDesignWorkEntries { get; set; } = new List<TrnDesignWorkEntry>();

    public virtual ICollection<TrnPlateMakingEntry> TrnPlateMakingEntries { get; set; } = new List<TrnPlateMakingEntry>();

    public virtual ICollection<TrnPrintWorkEntry> TrnPrintWorkEntries { get; set; } = new List<TrnPrintWorkEntry>();

    public virtual ICollection<TrnWorkspaceTaskItem> TrnWorkspaceTaskItems { get; set; } = new List<TrnWorkspaceTaskItem>();

    public virtual MstUser User { get; set; } = null!;

    public virtual MstWorkflowStep? WorkflowStep { get; set; }

    public virtual MstWorkflowTemplate? WorkflowTemplate { get; set; }
}
