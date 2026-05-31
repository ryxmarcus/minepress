using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Workflow definitions for job routing. Each template links a Job Type + Product Type to a sequence of steps.
/// </summary>
public partial class MstWorkflowTemplate
{
    public long WorkflowTemplateId { get; set; }

    public string WorkflowCode { get; set; } = null!;

    public string WorkflowName { get; set; } = null!;

    public string? Description { get; set; }

    public int? JobTypeId { get; set; }

    public int? PrintProductTypeId { get; set; }

    public bool IsDefault { get; set; }

    public int Version { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstJobType? JobType { get; set; }

    public virtual ICollection<MstWorkflowConnection> MstWorkflowConnections { get; set; } = new List<MstWorkflowConnection>();

    public virtual ICollection<MstWorkflowStep> MstWorkflowSteps { get; set; } = new List<MstWorkflowStep>();

    public virtual MstPrintProductType? PrintProductType { get; set; }

    public virtual ICollection<TrnWorkspaceTask> TrnWorkspaceTasks { get; set; } = new List<TrnWorkspaceTask>();
}
