using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Generic user activity log capturing all actions across the complete ERP system. Tracks CRUD operations, approvals, status changes, exports, prints, logins, navigation, and any user-initiated action. Supports audit trail with before/after value snapshots. Uses JSONB for flexible activity metadata.
/// </summary>
public partial class TrnUserActivityLog
{
    public long ActivityLogId { get; set; }

    public long UserId { get; set; }

    public string UserCode { get; set; } = null!;

    public string? UserName { get; set; }

    public DateTime ActivityOn { get; set; }

    /// <summary>
    /// ERP module: JOB, ENQUIRY, QUOTATION, USER_MGMT, MASTER, CRM, DISPATCH, PAYMENT, QUALITY, STOCK, MACHINE_SCHEDULE, RATE_CALC, REPORT, AUTH, NOTIFICATION, DASHBOARD, SETTINGS
    /// </summary>
    public string Module { get; set; } = null!;

    public string? SubModule { get; set; }

    /// <summary>
    /// Action type: CREATE, UPDATE, DELETE, VIEW, APPROVE, REJECT, PRINT, EXPORT, IMPORT, LOGIN, LOGOUT, STATUS_CHANGE, ASSIGN, UPLOAD, DOWNLOAD, SEND, CANCEL, CLOSE, REOPEN
    /// </summary>
    public string ActivityType { get; set; } = null!;

    /// <summary>
    /// Category: DATA (CRUD), AUTH (login/logout), NAVIGATION (page views), REPORT (report generation), APPROVAL (workflow), COMMUNICATION (email/sms/whatsapp), SYSTEM (background jobs)
    /// </summary>
    public string? ActivityCategory { get; set; }

    public string? EntityType { get; set; }

    public long? EntityId { get; set; }

    public string? EntityCode { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    /// <summary>
    /// JSONB snapshot of previous field values before an UPDATE or DELETE. Used for audit trail and change history.
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// JSONB snapshot of new field values after a CREATE or UPDATE. Used for audit trail and change history.
    /// </summary>
    public string? NewValues { get; set; }

    public List<string>? ChangedFields { get; set; }

    /// <summary>
    /// Flexible JSONB for context-specific metadata: report parameters, export format, filter criteria, approval remarks, etc.
    /// </summary>
    public string? ActivityData { get; set; }

    public string? RelatedEntityType { get; set; }

    public long? RelatedEntityId { get; set; }

    public string? RelatedEntityCode { get; set; }

    public long? JobId { get; set; }

    public int? ProcessId { get; set; }

    public int? SubprocessId { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? Channel { get; set; }

    public string? RequestPath { get; set; }

    public string? HttpMethod { get; set; }

    public string? CorrelationId { get; set; }

    public string? SessionId { get; set; }

    public string? DeviceInfo { get; set; }

    public int? CompanyId { get; set; }

    public int? LocationId { get; set; }

    public bool? IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    /// <summary>
    /// INFO: normal operations, WARNING: unusual activity, CRITICAL: security-sensitive actions, AUDIT: compliance-required logging
    /// </summary>
    public string? Severity { get; set; }

    public int? DurationMs { get; set; }

    public bool? IsArchived { get; set; }
}
