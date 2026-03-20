using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.VMS.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.VMS;

public class ContingentWorkerConfiguration : IEntityTypeConfiguration<ContingentWorker>
{
    public void Configure(EntityTypeBuilder<ContingentWorker> builder)
    {
        builder.ToTable("contingent_workers", "vms");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Namn).HasColumnName("namn").HasMaxLength(200).IsRequired();
        builder.Property(e => e.VendorId).HasColumnName("vendor_id");
        builder.Property(e => e.StaffingRequestId).HasColumnName("staffing_request_id");
        builder.Property(e => e.Tilltradesdatum).HasColumnName("tilltradesdatum");
        builder.Property(e => e.Slutdatum).HasColumnName("slutdatum");
        builder.Property(e => e.TimKostnad).HasColumnName("tim_kostnad").HasColumnType("decimal(18,2)");
        builder.Property(e => e.EnhetId).HasColumnName("enhet_id");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
    }
}
