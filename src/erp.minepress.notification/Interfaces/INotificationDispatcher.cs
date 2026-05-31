using erp.minepress.notification.Enums;
using erp.minepress.notification.Models;

namespace erp.minepress.notification.Interfaces;

/// <summary>
/// Context carrying recipient information needed by the dispatcher
/// to route notifications to the correct channels.
/// </summary>
public class NotificationContext
{
    /// <summary>
    /// Optional explicit key to group related notifications into a single email thread.
    /// Example: ENQ:ENQ-0001, QUOT:Q-1023, JOB:JOB-778.
    /// </summary>
    public string? ThreadKey { get; set; }

    // ── Internal recipients ──
    public int? AssigneeUserId { get; set; }
    public string? AssigneeEmail { get; set; }
    public string? AssigneePhone { get; set; }

    public int? DeptHeadUserId { get; set; }
    public string? DeptHeadEmail { get; set; }
    public string? DeptHeadPhone { get; set; }

    public int? SupervisorUserId { get; set; }
    public string? SupervisorEmail { get; set; }
    public string? SupervisorPhone { get; set; }

    public int? ApproverUserId { get; set; }
    public string? ApproverEmail { get; set; }
    public string? ApproverPhone { get; set; }

    // ── Client recipients ──
    public string? ClientEmail { get; set; }
    public string? ClientPhone { get; set; }

    /// <summary>
    /// Template variables for {{placeholder}} replacement.
    /// </summary>
    public Dictionary<string, string> Variables { get; set; } = [];
}

/// <summary>
/// Dispatches notifications for a process event based on the process notification config.
/// Reads the config flags (notify_client_email, notify_internal_sms, etc.)
/// and sends to all enabled channels using the referenced template.
/// </summary>
public interface INotificationDispatcher
{
    Task<IReadOnlyList<NotificationResult>> DispatchAsync(
        ProcessNotificationConfig config,
        NotificationTemplate template,
        NotificationContext context,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationResult>> DispatchAsync(
        ProcessNotificationConfig config,
        IReadOnlyList<NotificationTemplate> templates,
        NotificationContext context,
        CancellationToken cancellationToken = default);
}
