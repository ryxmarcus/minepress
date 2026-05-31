using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class TrnReceipt
{
    public long ReceiptId { get; set; }

    public string ReceiptNo { get; set; } = null!;

    public DateOnly ReceiptDate { get; set; }

    public int CompanyId { get; set; }

    public int? LocationId { get; set; }

    public int PartyId { get; set; }

    public string PaymentMode { get; set; } = null!;

    public string? ReferenceNo { get; set; }

    public DateOnly? ReferenceDate { get; set; }

    public int? BankId { get; set; }

    public decimal Amount { get; set; }

    public string? Remarks { get; set; }

    public string? Status { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstBankAccount? Bank { get; set; }

    public virtual MstCompany Company { get; set; } = null!;

    public virtual MstUser CreatedByNavigation { get; set; } = null!;

    public virtual MstParty Party { get; set; } = null!;

    public virtual ICollection<TrnReceiptAllocation> TrnReceiptAllocations { get; set; } = new List<TrnReceiptAllocation>();
}
