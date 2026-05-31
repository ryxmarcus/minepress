using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstItem
{
    public long ItemId { get; set; }

    public string ItemCode { get; set; } = null!;

    public string ItemName { get; set; } = null!;

    public string? ItemDescription { get; set; }

    public long? ItemCategoryId { get; set; }

    public long? ItemGroupId { get; set; }

    public long? ItemSubgroupId { get; set; }

    public long? BrandId { get; set; }

    public int? UomId { get; set; }

    public int? AlternateUomId { get; set; }

    public decimal? ConversionFactor { get; set; }

    public decimal? PurchaseRate { get; set; }

    public decimal? SalesRate { get; set; }

    public decimal? Mrp { get; set; }

    public decimal? MinimumRate { get; set; }

    public decimal? MaximumRate { get; set; }

    public decimal? OpeningQty { get; set; }

    public decimal? OpeningValue { get; set; }

    public decimal? ReorderLevel { get; set; }

    public decimal? ReorderQty { get; set; }

    public decimal? MinimumStockQty { get; set; }

    public decimal? MaximumStockQty { get; set; }

    public int? DefaultLocationId { get; set; }

    public int? DefaultCompanyId { get; set; }

    public int? TaxCategoryId { get; set; }

    public string? HsnCode { get; set; }

    public decimal? GstRate { get; set; }

    public bool? IsTaxInclusive { get; set; }

    public bool? IsPurchaseItem { get; set; }

    public bool? IsSalesItem { get; set; }

    public bool? IsInventoryItem { get; set; }

    public bool? IsServiceItem { get; set; }

    public string? CostMethod { get; set; }

    public decimal? LastPurchaseRate { get; set; }

    public DateOnly? LastPurchaseDate { get; set; }

    public decimal? StandardCost { get; set; }

    public bool? IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public string? Barcode { get; set; }

    public string? Sku { get; set; }

    public string? ModelNo { get; set; }

    public string? Specification { get; set; }

    public string? Remarks { get; set; }

    public virtual MstUom? AlternateUom { get; set; }

    public virtual MstBrand? Brand { get; set; }

    public virtual MstCompany? DefaultCompany { get; set; }

    public virtual MstLocation? DefaultLocation { get; set; }

    public virtual MstItemCategory? ItemCategory { get; set; }

    public virtual MstItemGroup? ItemGroup { get; set; }

    public virtual MstItemSubgroup? ItemSubgroup { get; set; }

    public virtual MstTaxCategory? TaxCategory { get; set; }

    public virtual ICollection<TrnSalesInvoiceItem> TrnSalesInvoiceItems { get; set; } = new List<TrnSalesInvoiceItem>();

    public virtual MstUom? Uom { get; set; }
}
