using erp.minepress.domain.Party;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace erp.minepress.persistence.Configurations;

public class PartyConfiguration : IEntityTypeConfiguration<PartyEntity>
{
    public void Configure(EntityTypeBuilder<PartyEntity> builder)
    {
        builder.ToTable("mst_party");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Code).HasColumnName("code").HasMaxLength(50);
        builder.Property(e => e.Address1).HasColumnName("address1").HasMaxLength(200);
        builder.Property(e => e.Address2).HasColumnName("address2").HasMaxLength(200);
        builder.Property(e => e.CityId).HasColumnName("city_id");
        builder.Property(e => e.Pin).HasColumnName("pin").HasMaxLength(10);
        builder.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
        builder.Property(e => e.Mobile).HasColumnName("mobile");
        builder.Property(e => e.GstNo).HasColumnName("gstno").HasMaxLength(20);
        builder.Property(e => e.PanNo).HasColumnName("pan_no").HasMaxLength(20);
        builder.Property(e => e.IsActive).HasColumnName("is_active");
        builder.Property(e => e.CreatedOn).HasColumnName("created_on");

        builder.HasMany(e => e.Contacts).WithOne(c => c.Party).HasForeignKey(c => c.PartyId);
        builder.HasMany(e => e.Addresses).WithOne(a => a.Party).HasForeignKey(a => a.PartyId);
        builder.HasMany(e => e.Roles).WithOne(r => r.Party).HasForeignKey(r => r.PartyId);
        builder.HasMany(e => e.Banks).WithOne(b => b.Party).HasForeignKey(b => b.PartyId);
    }
}
