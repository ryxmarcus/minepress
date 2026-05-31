using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnStoreIssue
{
    public long IssueId { get; set; }

    public string IssueNo { get; set; } = null!;

    public DateOnly IssueDate { get; set; }

    public string IssueType { get; set; } = null!;

    public long? JobId { get; set; }

    public string? JobNo { get; set; }

    public long? RateCalcId { get; set; }

    public int? FromLocationId { get; set; }

    public int? ToLocationId { get; set; }

    public int CompanyId { get; set; }

    public int? TotalItems { get; set; }

    public decimal? TotalAmount { get; set; }

    public string Status { get; set; } = null!;

    public string? Remarks { get; set; }

    public long? ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<TrnStoreIssueItem> TrnStoreIssueItems { get; set; } = new List<TrnStoreIssueItem>();
}
