using erp.minepress.domain.Common;

namespace erp.minepress.domain.Company;

public class CompanyEntity : AuditableEntity<int>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
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
    public string? EmailId { get; set; }
    public string? Website { get; set; }
    public int? CurrencyId { get; set; }
    public int? BaseCurrencyId { get; set; }
    public DateTime? FinYearStart { get; set; }
    public DateTime? FinYearEnd { get; set; }
    public DateTime? BooksStartDate { get; set; }
    public string? TaxRegime { get; set; }
    public int? DefaultTaxCategoryId { get; set; }
    public string? LogoUrl { get; set; }
    public string? PrintHeaderText { get; set; }
    public string? PrintFooterText { get; set; }
    public int? ParentCompanyId { get; set; }
    public bool IsGroupCompany { get; set; }
}
