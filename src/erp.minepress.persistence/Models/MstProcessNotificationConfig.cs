using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Hybrid master: drives ALL notifications (task, approval, client, AI, overdue, escalation)
///  for every subprocess across all 20 job processes.
///  Relational columns → fast filter/join. JSONB → flexible payload &amp; AI configuration.
/// </summary>
public partial class MstProcessNotificationConfig
{
    public long ConfigId { get; set; }

    public int JobTypeId { get; set; }

    public string JobTypeCode { get; set; } = null!;

    public int ProcessId { get; set; }

    public int SubprocessId { get; set; }

    public string ProcessCode { get; set; } = null!;

    public string SubprocessCode { get; set; } = null!;

    public long DepartmentId { get; set; }

    public string EventTypeCode { get; set; } = null!;

    public string EventLabel { get; set; } = null!;

    public int? ApprovalTypeId { get; set; }

    public int? ApprovalLevel { get; set; }

    public string RecipientType { get; set; } = null!;

    public bool NotifyAssignee { get; set; }

    public bool NotifyDeptHead { get; set; }

    public bool NotifySupervisor { get; set; }

    public bool NotifyApprover { get; set; }

    public bool NotifyClientSms { get; set; }

    public bool NotifyClientWhatsapp { get; set; }

    public bool NotifyClientEmail { get; set; }

    public bool NotifyInternalSms { get; set; }

    public bool NotifyInternalWhatsapp { get; set; }

    public bool NotifyInternalEmail { get; set; }

    public bool NotifyPush { get; set; }

    public bool NotifyTopupAlert { get; set; }

    public string? TemplateCode { get; set; }

    public string? SubjectTemplate { get; set; }

    public string? BodyTemplate { get; set; }

    public decimal? SlaHours { get; set; }

    public decimal? EscalateAfterHours { get; set; }

    public string? EscalateTo { get; set; }

    public decimal? OverdueReminderIntervalHours { get; set; }

    public string? TriggerOnStatus { get; set; }

    public bool AutoTrigger { get; set; }

    public string? TriggerCondition { get; set; }

    /// <summary>
    /// JSONB: template variables list, per-channel enable/retry rules, recipient routing hints.
    /// </summary>
    public string PayloadConfig { get; set; } = null!;

    /// <summary>
    /// JSONB: AI model, prompt template, event category, auto-assign flag, threshold, fallback.
    /// </summary>
    public string AiConfig { get; set; } = null!;

    public string Meta { get; set; } = null!;

    public string Priority { get; set; } = null!;

    public bool IsMandatory { get; set; }

    public bool IsActive { get; set; }

    public int SequenceNo { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstApprovalType? ApprovalType { get; set; }

    public virtual MstDepartment Department { get; set; } = null!;

    public virtual MstJobType JobType { get; set; } = null!;

    public virtual MstProcess Process { get; set; } = null!;
}
