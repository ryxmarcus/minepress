using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstParty
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Code { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public int? CityId { get; set; }

    public string? Pin { get; set; }

    public string? Email { get; set; }

    public long? Mobile { get; set; }

    public string? Gstno { get; set; }

    public string? PanNo { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual ICollection<HybJobRateCalculator> HybJobRateCalculators { get; set; } = new List<HybJobRateCalculator>();

    public virtual ICollection<MstCustomer> MstCustomers { get; set; } = new List<MstCustomer>();

    public virtual ICollection<MstPartyAddress> MstPartyAddresses { get; set; } = new List<MstPartyAddress>();

    public virtual ICollection<MstPartyBank> MstPartyBanks { get; set; } = new List<MstPartyBank>();

    public virtual ICollection<MstPartyContact> MstPartyContacts { get; set; } = new List<MstPartyContact>();

    public virtual ICollection<MstPartyRole> MstPartyRoles { get; set; } = new List<MstPartyRole>();

    public virtual ICollection<MstPartyTax> MstPartyTaxes { get; set; } = new List<MstPartyTax>();

    public virtual ICollection<MstSupplier> MstSuppliers { get; set; } = new List<MstSupplier>();

    public virtual ICollection<MstVendor> MstVendors { get; set; } = new List<MstVendor>();

    public virtual ICollection<PartyActivityLog> PartyActivityLogs { get; set; } = new List<PartyActivityLog>();

    public virtual ICollection<TrnAccountLedger> TrnAccountLedgers { get; set; } = new List<TrnAccountLedger>();

    public virtual ICollection<TrnAdvanceLedger> TrnAdvanceLedgers { get; set; } = new List<TrnAdvanceLedger>();

    public virtual ICollection<TrnApOutstanding> TrnApOutstandings { get; set; } = new List<TrnApOutstanding>();

    public virtual ICollection<TrnArOutstanding> TrnArOutstandings { get; set; } = new List<TrnArOutstanding>();

    public virtual ICollection<TrnChallan> TrnChallans { get; set; } = new List<TrnChallan>();

    public virtual ICollection<TrnCreditNote> TrnCreditNotes { get; set; } = new List<TrnCreditNote>();

    public virtual ICollection<TrnDebitNote> TrnDebitNotes { get; set; } = new List<TrnDebitNote>();

    public virtual ICollection<TrnEnquiry> TrnEnquiries { get; set; } = new List<TrnEnquiry>();

    public virtual ICollection<TrnGoodsReceipt> TrnGoodsReceipts { get; set; } = new List<TrnGoodsReceipt>();

    public virtual ICollection<TrnJob> TrnJobs { get; set; } = new List<TrnJob>();

    public virtual ICollection<TrnLedger> TrnLedgers { get; set; } = new List<TrnLedger>();

    public virtual ICollection<TrnPayment> TrnPayments { get; set; } = new List<TrnPayment>();

    public virtual ICollection<TrnProformaInvoice> TrnProformaInvoices { get; set; } = new List<TrnProformaInvoice>();

    public virtual ICollection<TrnPurchaseInvoice> TrnPurchaseInvoices { get; set; } = new List<TrnPurchaseInvoice>();

    public virtual ICollection<TrnPurchaseOrder> TrnPurchaseOrders { get; set; } = new List<TrnPurchaseOrder>();

    public virtual ICollection<TrnQuotation> TrnQuotations { get; set; } = new List<TrnQuotation>();

    public virtual ICollection<TrnReceipt> TrnReceipts { get; set; } = new List<TrnReceipt>();

    public virtual ICollection<TrnSalesInvoice> TrnSalesInvoices { get; set; } = new List<TrnSalesInvoice>();

    public virtual ICollection<TrnTdsLedger> TrnTdsLedgers { get; set; } = new List<TrnTdsLedger>();
}
