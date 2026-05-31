using System;
using System.Collections.Generic;

namespace erp.minepress.persistence.Models;

public partial class MstPartyAddress
{
    public int AddressId { get; set; }

    public int PartyId { get; set; }

    public string AddressType { get; set; } = null!;

    public string? AddressLabel { get; set; }

    public bool? IsDefault { get; set; }

    public bool? IsActive { get; set; }

    public string AddressLine1 { get; set; } = null!;

    public string? AddressLine2 { get; set; }

    public string? Landmark { get; set; }

    public int? CountryId { get; set; }

    public int? StateId { get; set; }

    public int? CityId { get; set; }

    public string? DistrictName { get; set; }

    public string? PostalCode { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string? ContactPersonName { get; set; }

    public string? ContactDesignation { get; set; }

    public string? ContactPhone { get; set; }

    public string? ContactEmail { get; set; }

    public string? Gstin { get; set; }

    public string? PanNo { get; set; }

    public string? TaxRegionCode { get; set; }

    public string? DeliveryInstructions { get; set; }

    public string? PreferredCarrier { get; set; }

    public string? DeliveryTimeSlot { get; set; }

    public bool? GeoTagVerified { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public virtual MstCity? City { get; set; }

    public virtual MstCountry? Country { get; set; }

    public virtual MstParty Party { get; set; } = null!;

    public virtual MstState? State { get; set; }

    public virtual ICollection<TrnSalesInvoice> TrnSalesInvoiceBillingAddresses { get; set; } = new List<TrnSalesInvoice>();

    public virtual ICollection<TrnSalesInvoice> TrnSalesInvoiceShippingAddresses { get; set; } = new List<TrnSalesInvoice>();
}
