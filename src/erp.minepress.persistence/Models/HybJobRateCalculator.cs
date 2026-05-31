using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Hybrid SQL+JSONB table storing AI Rate Calculator results. Relational columns for product config, IDs, totals (joins/filters/reporting). JSONB columns for parts, cost breakdown, BOM, AI insights, machine recommendations, and full input snapshot. Links to enquiry received process for quotation and negotiation workflow.
/// </summary>
public partial class HybJobRateCalculator
{
    public long RateCalcId { get; set; }

    /// <summary>
    /// Unique reference number generated as RC-YYYYMMDD-HHMMSS.
    /// </summary>
    public string CalcRefNo { get; set; } = null!;

    public long? EnquiryId { get; set; }

    public long? QuotationId { get; set; }

    public long? JobId { get; set; }

    public int? PartyId { get; set; }

    public int? JobTypeId { get; set; }

    public int? ProductTypeId { get; set; }

    public int? ProductSizeId { get; set; }

    public int Quantity { get; set; }

    public int TotalPages { get; set; }

    public decimal? TrimWidthMm { get; set; }

    public decimal? TrimHeightMm { get; set; }

    public string? PrintingMode { get; set; }

    public bool? IsCustomerMaterial { get; set; }

    public decimal GrandTotal { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal NetTotal { get; set; }

    public decimal CostPerUnit { get; set; }

    /// <summary>
    /// JSONB array of product parts with per-part configuration (pages, copies, colors, paper, finishing) and calculated results (sheets, paper cost, plate cost, ink cost, finishing cost, sub-total).
    /// </summary>
    public string? PartsData { get; set; }

    /// <summary>
    /// JSONB array of cost line items displayed in the Cost Breakdown table. Each item has icon, name, category, detail, and amount.
    /// </summary>
    public string? CostBreakdown { get; set; }

    /// <summary>
    /// JSONB array of Bill of Materials line items. Each item has category, material_name, specification, for_part, quantity, unit, rate, and amount.
    /// </summary>
    public string? BomData { get; set; }

    /// <summary>
    /// JSONB array of AI-generated insights/recommendations. Each has icon, title, description, and severity (info/warn/error).
    /// </summary>
    public string? AiInsights { get; set; }

    /// <summary>
    /// JSONB array of machine options with estimated costs for comparison.
    /// </summary>
    public string? RecommendedMachines { get; set; }

    /// <summary>
    /// JSONB snapshot of all selected master data at calculation time for reproducibility and audit trail.
    /// </summary>
    public string? CalcInputSnapshot { get; set; }

    public string Status { get; set; } = null!;

    public DateOnly? ValidityDate { get; set; }

    public int Version { get; set; }

    /// <summary>
    /// Self-referencing FK to previous version when a rate calculation is revised.
    /// </summary>
    public long? ParentCalcId { get; set; }

    public string? InternalRemarks { get; set; }

    public string? ClientRemarks { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public string? ConfigData { get; set; }

    public virtual MstUser CreatedByNavigation { get; set; } = null!;

    public virtual TrnEnquiry? Enquiry { get; set; }

    public virtual ICollection<HybJobRateCalculator> InverseParentCalc { get; set; } = new List<HybJobRateCalculator>();

    public virtual TrnJob? Job { get; set; }

    public virtual MstJobType? JobType { get; set; }

    public virtual HybJobRateCalculator? ParentCalc { get; set; }

    public virtual MstParty? Party { get; set; }

    public virtual MstPrintProductSize? ProductSize { get; set; }

    public virtual MstPrintProductType? ProductType { get; set; }

    public virtual TrnQuotation? Quotation { get; set; }

    public virtual ICollection<TrnEnquiryItem> TrnEnquiryItems { get; set; } = new List<TrnEnquiryItem>();

    public virtual ICollection<TrnJobItem> TrnJobItems { get; set; } = new List<TrnJobItem>();

    public virtual ICollection<TrnJob> TrnJobs { get; set; } = new List<TrnJob>();

    public virtual ICollection<TrnQuotationItem> TrnQuotationItems { get; set; } = new List<TrnQuotationItem>();
}
