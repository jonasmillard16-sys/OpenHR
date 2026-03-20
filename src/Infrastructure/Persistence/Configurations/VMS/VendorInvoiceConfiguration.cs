using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.VMS.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.VMS;

public class VendorInvoiceConfiguration : IEntityTypeConfiguration<VendorInvoice>
{
    public void Configure(EntityTypeBuilder<VendorInvoice> builder)
    {
        builder.ToTable("vendor_invoices", "vms");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.VendorId).HasColumnName("vendor_id");
        builder.Property(e => e.Period).HasColumnName("period").HasMaxLength(20);
        builder.Property(e => e.Belopp).HasColumnName("belopp").HasColumnType("decimal(18,2)");
        builder.Property(e => e.MatchadMotTidrapporter).HasColumnName("matchad_mot_tidrapporter");
        builder.Property(e => e.Differens).HasColumnName("differens").HasColumnType("decimal(18,2)");
        builder.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(30);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
    }
}
