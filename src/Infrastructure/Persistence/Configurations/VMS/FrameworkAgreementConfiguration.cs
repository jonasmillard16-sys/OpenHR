using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.SharedKernel.Domain;
using RegionHR.VMS.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.VMS;

public class FrameworkAgreementConfiguration : IEntityTypeConfiguration<FrameworkAgreement>
{
    public void Configure(EntityTypeBuilder<FrameworkAgreement> builder)
    {
        builder.ToTable("framework_agreements", "vms");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, v => FrameworkAgreementId.From(v))
            .HasColumnName("id");

        builder.Property(e => e.VendorId).HasColumnName("vendor_id");
        builder.Property(e => e.GiltigFran).HasColumnName("giltig_fran");
        builder.Property(e => e.GiltigTill).HasColumnName("giltig_till");
        builder.Property(e => e.Avtalsvillkor).HasColumnName("avtalsvillkor").HasMaxLength(2000);
        builder.Property(e => e.UppságningstidManader).HasColumnName("uppsagningstid_manader");
        builder.Property(e => e.ForlangningsKlausul).HasColumnName("forlangnings_klausul").HasMaxLength(1000);
        builder.Property(e => e.Avtalsvarde).HasColumnName("avtalsvarde").HasColumnType("decimal(18,2)");

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);

        builder.HasMany(e => e.RateCards).WithOne().HasForeignKey(r => r.FrameworkAgreementId);

        builder.Ignore(e => e.DomainEvents);
    }
}
