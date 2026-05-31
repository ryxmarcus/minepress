using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstNotificationTemplate
{
    public int TemplateId { get; set; }

    public string TemplateCode { get; set; } = null!;

    public string TemplateName { get; set; } = null!;

    public string Module { get; set; } = null!;

    public string EventType { get; set; } = null!;

    public string Channel { get; set; } = null!;

    public string? SubjectTemplate { get; set; }

    public string BodyTemplate { get; set; } = null!;

    public string? BodyFormat { get; set; }

    public string? VariablesJson { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsAiEnabled { get; set; }

    public string? AiPromptTemplate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual ICollection<TrnNotification> TrnNotifications { get; set; } = new List<TrnNotification>();
}
