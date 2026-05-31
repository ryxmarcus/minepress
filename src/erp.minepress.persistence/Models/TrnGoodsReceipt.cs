using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Goods Receipt Note (GRN) for material received from suppliers. Links PO to purchase invoice. Supports quality check.
/// </summary>
public partial class TrnGoodsReceipt
{
    public long GrnId { get; set; }

    public string GrnNo { get; set; } = null!;

    public DateOnly GrnDate { get; set; }

    public int CompanyId { get; set; }

    public int? LocationId { get; set; }

    public int PartyId { get; set; }

    public int? SupplierId { get; set; }

    public long? PurchaseOrderId { get; set; }

    public string? PoNo { get; set; }

    public string? SupplierChallanNo { get; set; }

    public DateOnly? SupplierChallanDate { get; set; }

    public string? VehicleNo { get; set; }

    public decimal? TotalQuantity { get; set; }

    public decimal? TotalAcceptedQty { get; set; }

    public decimal? TotalRejectedQty { get; set; }

    public string Status { get; set; } = null!;

    public bool? IsQualityChecked { get; set; }

    public long? QualityCheckedBy { get; set; }

    public DateTime? QualityCheckedOn { get; set; }

    public string? Remarks { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstCompany Company { get; set; } = null!;

    public virtual MstParty Party { get; set; } = null!;

    public virtual TrnPurchaseOrder? PurchaseOrder { get; set; }

    public virtual ICollection<TrnGoodsReceiptItem> TrnGoodsReceiptItems { get; set; } = new List<TrnGoodsReceiptItem>();
}
