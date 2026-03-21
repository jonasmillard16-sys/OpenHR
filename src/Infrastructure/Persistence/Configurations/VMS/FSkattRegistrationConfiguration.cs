using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.VMS.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.VMS;

public class FSkattRegistrationConfiguration : IEntityTypeConfiguration<FSkattRegistration>
{
    public void Configure(EntityTypeBuilder<FSkattRegistration> builder)
    {
        builder.ToTable("fskatt_registrations", "vms");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.ContingentWorkerId).HasColumnName("contingent_worker_id");
        builder.Property(e => e.VendorId).HasColumnName("vendor_id");
        builder.Property(e => e.Organisationsnummer).HasColumnName("organisationsnummer").HasMaxLength(20).IsRequired();
        builder.Property(e => e.FSkattStatus).HasConversion<string>().HasColumnName("fskatt_status").HasMaxLength(20);
        builder.Property(e => e.KontrolleradVid).HasColumnName("kontrollerad_vid");
        builder.Property(e => e.GiltigTill).HasColumnName("giltig_till");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(e => e.VendorId);
        builder.HasIndex(e => e.ContingentWorkerId);
        builder.HasIndex(e => e.FSkattStatus);

        builder.Ignore(e => e.KräverSkatteavdrag);
    }
}
