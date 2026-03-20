using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.SharedKernel.Domain;
using RegionHR.VMS.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.VMS;

public class StaffingRequestConfiguration : IEntityTypeConfiguration<StaffingRequest>
{
    public void Configure(EntityTypeBuilder<StaffingRequest> builder)
    {
        builder.ToTable("staffing_requests", "vms");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, v => StaffingRequestId.From(v))
            .HasColumnName("id");

        builder.Property(e => e.EnhetId).HasColumnName("enhet_id");
        builder.Property(e => e.Befattning).HasColumnName("befattning").HasMaxLength(200).IsRequired();
        builder.Property(e => e.PeriodFran).HasColumnName("period_fran");
        builder.Property(e => e.PeriodTill).HasColumnName("period_till");
        builder.Property(e => e.AntalPersoner).HasColumnName("antal_personer");
        builder.Property(e => e.Kravprofil).HasColumnName("kravprofil").HasMaxLength(2000);
        builder.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(30);

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);

        builder.Ignore(e => e.DomainEvents);
        builder.Ignore(e => e.Version);
    }
}
