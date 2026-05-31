using erp.minepress.domain.Common;

namespace erp.minepress.domain.Inventory;

public class ItemEntity : AuditableEntity<long>
{
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? ItemDescription { get; set; }
    public long? ItemCategoryId { get; set; }
    public long? ItemGroupId { get; set; }
    public long? ItemSubgroupId { get; set; }
    public long? BrandId { get; set; }
    public int? UomId { get; set; }
    public int? AlternateUomId { get; set; }
    public decimal ConversionFactor { get; set; } = 1.0m;
    public decimal? PurchaseRate { get; set; }
    public decimal? SalesRate { get; set; }
    public decimal? Mrp { get; set; }
    public decimal OpeningQty { get; set; }
    public decimal OpeningValue { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal ReorderQty { get; set; }
    public decimal MinimumStockQty { get; set; }
    public decimal MaximumStockQty { get; set; }
    public int? DefaultLocationId { get; set; }
    public int? DefaultCompanyId { get; set; }
    public int? TaxCategoryId { get; set; }
    public string? HsnCode { get; set; }
    public decimal? GstRate { get; set; }
    public bool IsTaxInclusive { get; set; }
    public bool IsPurchaseItem { get; set; } = true;
    public bool IsSalesItem { get; set; } = true;
    public bool IsInventoryItem { get; set; } = true;
    public bool IsServiceItem { get; set; }
    public string CostMethod { get; set; } = "FIFO";
    public decimal? LastPurchaseRate { get; set; }
    public DateTime? LastPurchaseDate { get; set; }
    public decimal? StandardCost { get; set; }
    public string? Barcode { get; set; }
    public string? Sku { get; set; }
    public string? Specification { get; set; }
    public string? Remarks { get; set; }
}
