using erp.minepress.domain.Company;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace erp.minepress.persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<CompanyEntity>
{
    public void Configure(EntityTypeBuilder<CompanyEntity> builder)
    {
        builder.ToTable("mst_company");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(e => e.LegalName).HasColumnName("legal_name").HasMaxLength(250);
        builder.Property(e => e.ShortName).HasColumnName("short_name").HasMaxLength(50);
        builder.Property(e => e.Gstin).HasColumnName("gstin").HasMaxLength(20);
        builder.Property(e => e.PanNo).HasColumnName("pan_no").HasMaxLength(20);
        builder.Property(e => e.IsActive).HasColumnName("is_active");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
        builder.Property(e => e.CreatedOn).HasColumnName("created_on");
        builder.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        builder.Property(e => e.ModifiedOn).HasColumnName("modified_on");
        builder.HasIndex(e => e.Code).IsUnique();
    }
}
