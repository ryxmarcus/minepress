using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Connections between workflow steps. Supports conditional branching via condition_expression.
/// </summary>
public partial class MstWorkflowConnection
{
    public long ConnectionId { get; set; }

    public long WorkflowTemplateId { get; set; }

    public long FromStepId { get; set; }

    public long ToStepId { get; set; }

    public string? ConditionExpression { get; set; }

    public string? Label { get; set; }

    public int SequenceNo { get; set; }

    public bool IsActive { get; set; }

    public virtual MstWorkflowStep FromStep { get; set; } = null!;

    public virtual MstWorkflowStep ToStep { get; set; } = null!;

    public virtual MstWorkflowTemplate WorkflowTemplate { get; set; } = null!;
}
