using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Tax ledger for GST compliance. One row per tax component per voucher line. Powers GSTR-1, GSTR-2B, GSTR-3B, and ITC reports. direction_id 1=Output (payable), 2=Input (ITC).
/// </summary>
public partial class TrnTaxLedger
{
    public long TaxLedgerId { get; set; }

    public int CompanyId { get; set; }

    public int? FinYearId { get; set; }

    /// <summary>
    /// GST return period in MMYYYY format e.g. 072025 for July 2025.
    /// </summary>
    public string? TaxPeriod { get; set; }

    public DateOnly PostingDate { get; set; }

    public int? TransactionTypeId { get; set; }

    public int DirectionId { get; set; }

    public string VoucherType { get; set; } = null!;

    public long VoucherId { get; set; }

    public string? VoucherNo { get; set; }

    public DateOnly? VoucherDate { get; set; }

    public int? PartyId { get; set; }

    public string? PartyGstin { get; set; }

    public string? PlaceOfSupply { get; set; }

    public string? HsnSacCode { get; set; }

    public decimal? TaxableValue { get; set; }

    public int TaxComponentId { get; set; }

    public decimal? TaxRate { get; set; }

    public decimal? TaxAmount { get; set; }

    public bool? IsReverseCharge { get; set; }

    public bool? IsNilRated { get; set; }

    public bool? IsExempt { get; set; }

    public bool? ItcEligible { get; set; }

    /// <summary>
    /// ITC category: INPUTS, CAPITAL_GOODS, INPUT_SERVICES, INELIGIBLE
    /// </summary>
    public string? ItcCategory { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual MstCompany Company { get; set; } = null!;

    public virtual MstDirection Direction { get; set; } = null!;

    public virtual MstTaxComponent TaxComponent { get; set; } = null!;

    public virtual MstTransactionType? TransactionType { get; set; }
}
