using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstCurrency
{
    public int CurrencyId { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public string CurrencyName { get; set; } = null!;

    public string? Symbol { get; set; }

    public int? CountryId { get; set; }

    public int? DecimalPlaces { get; set; }

    public string? SymbolPosition { get; set; }

    public string? ThousandSeparator { get; set; }

    public string? DecimalSeparator { get; set; }

    public bool? BaseCurrency { get; set; }

    public decimal? ExchangeRate { get; set; }

    public DateOnly? EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public string? RateSource { get; set; }

    public bool? IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public virtual ICollection<MstBankAccount> MstBankAccounts { get; set; } = new List<MstBankAccount>();

    public virtual ICollection<MstCompany> MstCompanyBaseCurrencies { get; set; } = new List<MstCompany>();

    public virtual ICollection<MstCompany> MstCompanyCurrencies { get; set; } = new List<MstCompany>();

    public virtual ICollection<MstSupplier> MstSuppliers { get; set; } = new List<MstSupplier>();

    public virtual ICollection<TrnAccountLedger> TrnAccountLedgers { get; set; } = new List<TrnAccountLedger>();

    public virtual ICollection<TrnSalesInvoice> TrnSalesInvoices { get; set; } = new List<TrnSalesInvoice>();
}
