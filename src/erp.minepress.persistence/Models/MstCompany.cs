using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstCompany
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? LegalName { get; set; }

    public string? ShortName { get; set; }

    public string? Description { get; set; }

    public string? RegistrationNo { get; set; }

    public string? PanNo { get; set; }

    public string? Gstin { get; set; }

    public string? CinNo { get; set; }

    public string? TanNo { get; set; }

    public string? IecCode { get; set; }

    public string? MsmeNo { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public int? CityId { get; set; }

    public int? StateId { get; set; }

    public int? CountryId { get; set; }

    public string? Pincode { get; set; }

    public string? ContactPerson { get; set; }

    public string? ContactNo { get; set; }

    public string? AltContactNo { get; set; }

    public string? EmailId { get; set; }

    public string? Website { get; set; }

    public int? CurrencyId { get; set; }

    public int? BaseCurrencyId { get; set; }

    public DateOnly? FinYearStart { get; set; }

    public DateOnly? FinYearEnd { get; set; }

    public DateOnly? BooksStartDate { get; set; }

    public string? TaxRegime { get; set; }

    public int? DefaultTaxCategoryId { get; set; }

    public string? BankName { get; set; }

    public string? BranchName { get; set; }

    public string? AccountNo { get; set; }

    public string? IfscCode { get; set; }

    public string? SwiftCode { get; set; }

    public string? LogoUrl { get; set; }

    public string? PrintHeaderText { get; set; }

    public string? PrintFooterText { get; set; }

    public int? ParentCompanyId { get; set; }

    public bool? IsGroupCompany { get; set; }

    public bool? IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual MstCurrency? BaseCurrency { get; set; }

    public virtual MstCity? City { get; set; }

    public virtual MstCountry? Country { get; set; }

    public virtual MstCurrency? Currency { get; set; }

    public virtual MstTaxCategory? DefaultTaxCategory { get; set; }

    public virtual ICollection<MstCompany> InverseParentCompany { get; set; } = new List<MstCompany>();

    public virtual ICollection<MstBankAccount> MstBankAccounts { get; set; } = new List<MstBankAccount>();

    public virtual ICollection<MstEmployee> MstEmployees { get; set; } = new List<MstEmployee>();

    public virtual ICollection<MstFinancialYear> MstFinancialYears { get; set; } = new List<MstFinancialYear>();

    public virtual ICollection<MstLocation> MstLocations { get; set; } = new List<MstLocation>();

    public virtual ICollection<MstUser> MstUsers { get; set; } = new List<MstUser>();

    public virtual MstCompany? ParentCompany { get; set; }

    public virtual MstState? State { get; set; }

    public virtual ICollection<TrnAccountLedger> TrnAccountLedgers { get; set; } = new List<TrnAccountLedger>();

    public virtual ICollection<TrnAdvanceLedger> TrnAdvanceLedgers { get; set; } = new List<TrnAdvanceLedger>();

    public virtual ICollection<TrnApOutstanding> TrnApOutstandings { get; set; } = new List<TrnApOutstanding>();

    public virtual ICollection<TrnArOutstanding> TrnArOutstandings { get; set; } = new List<TrnArOutstanding>();

    public virtual ICollection<TrnBankPayment> TrnBankPayments { get; set; } = new List<TrnBankPayment>();

    public virtual ICollection<TrnBankReceipt> TrnBankReceipts { get; set; } = new List<TrnBankReceipt>();

    public virtual ICollection<TrnBankReconciliation> TrnBankReconciliations { get; set; } = new List<TrnBankReconciliation>();

    public virtual ICollection<TrnChallan> TrnChallans { get; set; } = new List<TrnChallan>();

    public virtual ICollection<TrnContraVoucher> TrnContraVouchers { get; set; } = new List<TrnContraVoucher>();

    public virtual ICollection<TrnCreditNote> TrnCreditNotes { get; set; } = new List<TrnCreditNote>();

    public virtual ICollection<TrnDebitNote> TrnDebitNotes { get; set; } = new List<TrnDebitNote>();

    public virtual ICollection<TrnEnquiry> TrnEnquiries { get; set; } = new List<TrnEnquiry>();

    public virtual ICollection<TrnExpenseVoucher> TrnExpenseVouchers { get; set; } = new List<TrnExpenseVoucher>();

    public virtual ICollection<TrnGatePass> TrnGatePasses { get; set; } = new List<TrnGatePass>();

    public virtual ICollection<TrnGoodsReceipt> TrnGoodsReceipts { get; set; } = new List<TrnGoodsReceipt>();

    public virtual ICollection<TrnJob> TrnJobs { get; set; } = new List<TrnJob>();

    public virtual ICollection<TrnJournalVoucher> TrnJournalVouchers { get; set; } = new List<TrnJournalVoucher>();

    public virtual ICollection<TrnPayment> TrnPayments { get; set; } = new List<TrnPayment>();

    public virtual ICollection<TrnProformaInvoice> TrnProformaInvoices { get; set; } = new List<TrnProformaInvoice>();

    public virtual ICollection<TrnPurchaseInvoice> TrnPurchaseInvoices { get; set; } = new List<TrnPurchaseInvoice>();

    public virtual ICollection<TrnPurchaseOrder> TrnPurchaseOrders { get; set; } = new List<TrnPurchaseOrder>();

    public virtual ICollection<TrnQuotation> TrnQuotations { get; set; } = new List<TrnQuotation>();

    public virtual ICollection<TrnReceipt> TrnReceipts { get; set; } = new List<TrnReceipt>();

    public virtual ICollection<TrnSalesInvoice> TrnSalesInvoices { get; set; } = new List<TrnSalesInvoice>();

    public virtual ICollection<TrnTaxLedger> TrnTaxLedgers { get; set; } = new List<TrnTaxLedger>();

    public virtual ICollection<TrnTdsLedger> TrnTdsLedgers { get; set; } = new List<TrnTdsLedger>();
}
