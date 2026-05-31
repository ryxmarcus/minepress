using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Centralized error log for capturing exceptions across all application layers
/// </summary>
public partial class SysErrorLog
{
    public long ErrorLogId { get; set; }

    /// <summary>
    /// Application layer: UI, API, Infrastructure, Application, Persistence
    /// </summary>
    public string Layer { get; set; } = null!;

    /// <summary>
    /// Source component: Controller name, Service class, Repository, Blazor component, etc.
    /// </summary>
    public string Source { get; set; } = null!;

    public string? MethodName { get; set; }

    public string ExceptionType { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string StackTrace { get; set; } = null!;

    public string InnerException { get; set; } = null!;

    public string RequestPath { get; set; } = null!;

    public string HttpMethod { get; set; } = null!;

    public string RequestData { get; set; } = null!;

    public long UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string IpAddress { get; set; } = null!;

    public string UserAgent { get; set; } = null!;

    /// <summary>
    /// Unique ID for tracing related log entries across services
    /// </summary>
    public string CorrelationId { get; set; } = null!;

    public string TenantKey { get; set; } = null!;

    /// <summary>
    /// Severity level: Critical, Error, Warning, Info
    /// </summary>
    public string Severity { get; set; } = null!;

    public string AdditionalData { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public string MachineName { get; set; } = null!;

    public string AppVersion { get; set; } = null!;

    public bool IsReviewed { get; set; }

    public string? ReviewNotes { get; set; }

    public string? ReviewedBy { get; set; }

    public DateTime? ReviewedOn { get; set; }
}
