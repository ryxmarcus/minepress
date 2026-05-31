using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Master table for expense categories: Office, Travel, Utilities, Repairs, Rent, Salary, Transport, Printing, Misc. Maps to account head for GL posting.
/// </summary>
public partial class MstExpenseCategory
{
    public int ExpenseCategoryId { get; set; }

    public string CategoryCode { get; set; } = null!;

    public string CategoryName { get; set; } = null!;

    public int? ParentCategoryId { get; set; }

    public long? AccountHeadId { get; set; }

    public string? Description { get; set; }

    public bool? IsReimbursable { get; set; }

    public bool? RequiresApproval { get; set; }

    public decimal? ApprovalLimit { get; set; }

    public int? TaxCategoryId { get; set; }

    public bool? IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstAccountHead? AccountHead { get; set; }

    public virtual ICollection<MstExpenseCategory> InverseParentCategory { get; set; } = new List<MstExpenseCategory>();

    public virtual MstExpenseCategory? ParentCategory { get; set; }
}
