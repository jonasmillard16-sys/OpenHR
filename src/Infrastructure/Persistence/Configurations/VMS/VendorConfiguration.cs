using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.SharedKernel.Domain;
using RegionHR.VMS.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.VMS;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("vendors", "vms");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, v => VendorId.From(v))
            .HasColumnName("id");

        builder.Property(e => e.Namn).HasColumnName("namn").HasMaxLength(200).IsRequired();
        builder.Property(e => e.OrgNummer).HasColumnName("org_nummer").HasMaxLength(20).IsRequired();
        builder.Property(e => e.Kontaktperson).HasColumnName("kontaktperson").HasMaxLength(200);
        builder.Property(e => e.Epost).HasColumnName("epost").HasMaxLength(200);
        builder.Property(e => e.Telefon).HasColumnName("telefon").HasMaxLength(50);
        builder.Property(e => e.Kategori).HasColumnName("kategori").HasMaxLength(100);
        builder.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(30);

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);

        builder.Ignore(e => e.DomainEvents);
        builder.Ignore(e => e.Version);
    }
}
