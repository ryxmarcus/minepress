using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// TDS (Tax Deducted at Source) ledger. Tracks TDS deducted on payments to suppliers/vendors. Powers TDS return filing (26Q/27Q), certificate generation.
/// </summary>
public partial class TrnTdsLedger
{
    public long TdsId { get; set; }

    public int CompanyId { get; set; }

    public int? FinYearId { get; set; }

    public int PartyId { get; set; }

    /// <summary>
    /// TDS section: 194C (Contractor), 194J (Professional), 194I (Rent), 194H (Commission), 194A (Interest), etc.
    /// </summary>
    public string TdsSection { get; set; } = null!;

    public decimal TdsRate { get; set; }

    public string VoucherType { get; set; } = null!;

    public long VoucherId { get; set; }

    public string? VoucherNo { get; set; }

    public DateOnly VoucherDate { get; set; }

    public decimal BaseAmount { get; set; }

    public decimal TdsAmount { get; set; }

    public decimal? SurchargeAmount { get; set; }

    public decimal? EducationCess { get; set; }

    public decimal? TotalTdsAmount { get; set; }

    public bool? IsDeposited { get; set; }

    public string? DepositChallanNo { get; set; }

    public DateOnly? DepositDate { get; set; }

    public string? BsrCode { get; set; }

    public string? CertificateNo { get; set; }

    public bool? IsReturnFiled { get; set; }

    /// <summary>
    /// TDS quarter: Q1 (Apr-Jun), Q2 (Jul-Sep), Q3 (Oct-Dec), Q4 (Jan-Mar)
    /// </summary>
    public string? Quarter { get; set; }

    public string? Narration { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual MstCompany Company { get; set; } = null!;

    public virtual MstParty Party { get; set; } = null!;
}
