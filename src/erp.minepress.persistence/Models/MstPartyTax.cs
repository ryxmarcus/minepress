using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

/// <summary>
/// Tax registrations per party: GSTIN, PAN, TAN, VAT numbers.
/// </summary>
public partial class MstPartyTax
{
    public int PartyTaxId { get; set; }

    public int PartyId { get; set; }

    public int TaxTypeId { get; set; }

    public string? TaxNumber { get; set; }

    public int? TaxRegionId { get; set; }

    public DateOnly? RegistrationDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public bool? IsDefault { get; set; }

    public bool? IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public virtual MstParty Party { get; set; } = null!;

    public virtual MstTaxRegion? TaxRegion { get; set; }

    public virtual MstTaxType TaxType { get; set; } = null!;
}
