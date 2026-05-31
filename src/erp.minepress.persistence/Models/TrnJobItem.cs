using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Line items for trn_job. Holds product identity, pricing, specs and a frozen cost_breakdown JSONB snapshot from the rate calculator at job time. Detailed calculation data lives in hyb_job_rate_calculator via rate_calculator_id.
/// </summary>
public partial class TrnJobItem
{
    public long JobItemId { get; set; }

    public long JobId { get; set; }

    public long? EnquiryItemId { get; set; }

    public long? QuotationItemId { get; set; }

    public int ItemSequence { get; set; }

    public int? PrintProductTypeId { get; set; }

    public int? JobTypeId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? ProductDescription { get; set; }

    public string? ProductTypeName { get; set; }

    public string? JobTypeName { get; set; }

    public string? ProductSizeName { get; set; }

    public decimal? TrimWidthMm { get; set; }

    public decimal? TrimHeightMm { get; set; }

    public string? PrintingMethod { get; set; }

    public int? UomId { get; set; }

    public int? NoOfPages { get; set; }

    public int? Quantity { get; set; }

    public int? DeliveredQuantity { get; set; }

    public int? PendingQuantity { get; set; }

    public long? RateCalculatorId { get; set; }

    /// <summary>
    /// Denormalized reference number from hyb_job_rate_calculator for quick display on PDF/UI.
    /// </summary>
    public string? CalcRefNo { get; set; }

    public decimal? GrossAmount { get; set; }

    public decimal? UnitRate { get; set; }

    public decimal? DiscountPercent { get; set; }

    public decimal? DiscountAmount { get; set; }

    public int? TaxCategoryId { get; set; }

    public string? HsnSacCode { get; set; }

    public decimal? TaxableValue { get; set; }

    public decimal? CgstPercent { get; set; }

    public decimal? CgstAmount { get; set; }

    public decimal? SgstPercent { get; set; }

    public decimal? SgstAmount { get; set; }

    public decimal? IgstPercent { get; set; }

    public decimal? IgstAmount { get; set; }

    public decimal? CessPercent { get; set; }

    public decimal? CessAmount { get; set; }

    public decimal? TotalTaxAmount { get; set; }

    public decimal? NetAmount { get; set; }

    public string Status { get; set; } = null!;

    public string? Remarks { get; set; }

    public string? InternalRemarks { get; set; }

    public int? SortOrder { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstUser CreatedByNavigation { get; set; } = null!;

    public virtual TrnJob Job { get; set; } = null!;

    public virtual MstJobType? JobType { get; set; }

    public virtual MstPrintProductType? PrintProductType { get; set; }

    public virtual HybJobRateCalculator? RateCalculator { get; set; }

    public virtual ICollection<TrnChallanItem> TrnChallanItems { get; set; } = new List<TrnChallanItem>();

    public virtual ICollection<TrnJobOutsourceItem> TrnJobOutsourceItems { get; set; } = new List<TrnJobOutsourceItem>();

    public virtual ICollection<TrnWorkspaceTaskItem> TrnWorkspaceTaskItems { get; set; } = new List<TrnWorkspaceTaskItem>();
}
