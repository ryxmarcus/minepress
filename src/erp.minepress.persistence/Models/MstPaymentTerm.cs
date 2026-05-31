using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstPaymentTerm
{
    public int PaymentTermId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? TermType { get; set; }

    public int? DueDays { get; set; }

    public decimal? DiscountPercent { get; set; }

    public int? DiscountDays { get; set; }

    public bool? IsDefault { get; set; }

    public string? ApplicableToPartyType { get; set; }

    public bool? IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual ICollection<TrnPurchaseInvoice> TrnPurchaseInvoices { get; set; } = new List<TrnPurchaseInvoice>();

    public virtual ICollection<TrnSalesInvoice> TrnSalesInvoices { get; set; } = new List<TrnSalesInvoice>();
}
