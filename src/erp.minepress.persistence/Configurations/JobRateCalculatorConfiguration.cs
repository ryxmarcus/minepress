using erp.minepress.domain.Job;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace erp.minepress.persistence.Configurations;

public class JobRateCalculatorConfiguration : IEntityTypeConfiguration<JobRateCalculatorEntity>
{
    public void Configure(EntityTypeBuilder<JobRateCalculatorEntity> builder)
    {
        builder.ToTable("hyb_job_rate_calculator");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("rate_calc_id");
        builder.Property(e => e.CalcRefNo).HasColumnName("calc_ref_no").HasMaxLength(50).IsRequired();
        builder.Property(e => e.EnquiryId).HasColumnName("enquiry_id");
        builder.Property(e => e.QuotationId).HasColumnName("quotation_id");
        builder.Property(e => e.JobId).HasColumnName("job_id");
        builder.Property(e => e.PartyId).HasColumnName("party_id");
        builder.Property(e => e.JobTypeId).HasColumnName("job_type_id");
        builder.Property(e => e.ProductTypeId).HasColumnName("product_type_id");
        builder.Property(e => e.ProductSizeId).HasColumnName("product_size_id");
        builder.Property(e => e.Quantity).HasColumnName("quantity");
        builder.Property(e => e.TotalPages).HasColumnName("total_pages");
        builder.Property(e => e.TrimWidthMm).HasColumnName("trim_width_mm").HasColumnType("numeric(8,2)");
        builder.Property(e => e.TrimHeightMm).HasColumnName("trim_height_mm").HasColumnType("numeric(8,2)");
        builder.Property(e => e.PrintingMode).HasColumnName("printing_mode").HasMaxLength(30);
        builder.Property(e => e.IsCustomerMaterial).HasColumnName("is_customer_material");
        builder.Property(e => e.GrandTotal).HasColumnName("grand_total").HasColumnType("numeric(14,2)");
        builder.Property(e => e.TaxAmount).HasColumnName("tax_amount").HasColumnType("numeric(14,2)");
        builder.Property(e => e.NetTotal).HasColumnName("net_total").HasColumnType("numeric(14,2)");
        builder.Property(e => e.CostPerUnit).HasColumnName("cost_per_unit").HasColumnType("numeric(14,4)");
        builder.Property(e => e.PartsData).HasColumnName("parts_data").HasColumnType("jsonb");
        builder.Property(e => e.CostBreakdown).HasColumnName("cost_breakdown").HasColumnType("jsonb");
        builder.Property(e => e.BomData).HasColumnName("bom_data").HasColumnType("jsonb");
        builder.Property(e => e.AiInsights).HasColumnName("ai_insights").HasColumnType("jsonb");
        builder.Property(e => e.RecommendedMachines).HasColumnName("recommended_machines").HasColumnType("jsonb");
        builder.Property(e => e.CalcInputSnapshot).HasColumnName("calc_input_snapshot").HasColumnType("jsonb");
        builder.Property(e => e.Status).HasColumnName("status").HasMaxLength(30);
        builder.Property(e => e.ValidityDate).HasColumnName("validity_date");
        builder.Property(e => e.Version).HasColumnName("version");
        builder.Property(e => e.ParentCalcId).HasColumnName("parent_calc_id");
        builder.Property(e => e.InternalRemarks).HasColumnName("internal_remarks");
        builder.Property(e => e.ClientRemarks).HasColumnName("client_remarks");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.CreatedOn).HasColumnName("created_on");
        builder.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(50);
        builder.Property(e => e.ModifiedOn).HasColumnName("modified_on");
    }
}
