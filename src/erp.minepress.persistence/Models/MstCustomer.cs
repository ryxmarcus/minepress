using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstCustomer
{
    public int Id { get; set; }

    public int PartyId { get; set; }

    public int? CustomerType { get; set; }

    public int? CustomerGroup { get; set; }

    public int? PaymentTerms { get; set; }

    public int? DueDateBase { get; set; }

    public decimal? MaxCreditLimit { get; set; }

    public decimal? TotalUtilizedCreditLimitAmt { get; set; }

    public decimal? AvailableCreditLimitAmt { get; set; }

    public decimal? HoldCreditLimitAmt { get; set; }

    public decimal? SuspendedCreditLimitAmt { get; set; }

    public string? Salesperson { get; set; }

    public int? Language { get; set; }

    public int? Status { get; set; }

    public bool? IsActive { get; set; }

    public virtual MstCustomerGroup? CustomerGroupNavigation { get; set; }

    public virtual MstCustomerType? CustomerTypeNavigation { get; set; }

    public virtual MstParty Party { get; set; } = null!;

    public virtual MstCustomerPaymentTerm? PaymentTermsNavigation { get; set; }
}
