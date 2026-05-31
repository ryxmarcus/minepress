using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstJobType
{
    public int Jobtypeid { get; set; }

    public string Jobtypecode { get; set; } = null!;

    public string Jobtypename { get; set; } = null!;

    public string? Description { get; set; }

    public bool? Isdesignrequired { get; set; }

    public bool? Isdtprequired { get; set; }

    public bool? Isctprequired { get; set; }

    public bool? Isprintingrequired { get; set; }

    public bool? Isbindingrequired { get; set; }

    public bool? Isfinishingrequired { get; set; }

    public string? Printingmode { get; set; }

    public bool? Issingleprocess { get; set; }

    public bool? Isfullprocess { get; set; }

    public bool? Iscustomermaterial { get; set; }

    public bool? Isinhousematerial { get; set; }

    public bool? Isoutsourcejob { get; set; }

    public bool? Allowadvancepayment { get; set; }

    public bool? Requirecostingapproval { get; set; }

    public string? Defaultstartprocesscode { get; set; }

    public string? Defaultendprocesscode { get; set; }

    public bool? Isactive { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual ICollection<HybJobRateCalculator> HybJobRateCalculators { get; set; } = new List<HybJobRateCalculator>();

    public virtual ICollection<MstProcessNotificationConfig> MstProcessNotificationConfigs { get; set; } = new List<MstProcessNotificationConfig>();

    public virtual ICollection<MstWorkflowTemplate> MstWorkflowTemplates { get; set; } = new List<MstWorkflowTemplate>();

    public virtual ICollection<TrnJobItem> TrnJobItems { get; set; } = new List<TrnJobItem>();

    public virtual ICollection<TrnJob> TrnJobs { get; set; } = new List<TrnJob>();

    public virtual ICollection<TrnQuotationItem> TrnQuotationItems { get; set; } = new List<TrnQuotationItem>();
}
