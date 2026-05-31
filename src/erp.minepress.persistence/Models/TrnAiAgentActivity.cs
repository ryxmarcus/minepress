using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnAiAgentActivity
{
    public long ActivityId { get; set; }

    public string AgentName { get; set; } = null!;

    public string AgentAction { get; set; } = null!;

    public string? Module { get; set; }

    public int? ReferenceId { get; set; }

    public string? ReferenceNo { get; set; }

    public long? UserId { get; set; }

    public string? InputJson { get; set; }

    public string? OutputJson { get; set; }

    public decimal? ConfidenceScore { get; set; }

    public bool? WasAccepted { get; set; }

    public string? Feedback { get; set; }

    public int? ExecutionTimeMs { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual MstUser? User { get; set; }
}
