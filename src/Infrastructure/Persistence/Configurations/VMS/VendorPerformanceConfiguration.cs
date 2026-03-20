using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.VMS.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.VMS;

public class VendorPerformanceConfiguration : IEntityTypeConfiguration<VendorPerformance>
{
    public void Configure(EntityTypeBuilder<VendorPerformance> builder)
    {
        builder.ToTable("vendor_performances", "vms");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.VendorId).HasColumnName("vendor_id");
        builder.Property(e => e.Period).HasColumnName("period").HasMaxLength(20);
        builder.Property(e => e.Poang).HasColumnName("poang");
        builder.Property(e => e.Kommentar).HasColumnName("kommentar").HasMaxLength(2000);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
    }
}
