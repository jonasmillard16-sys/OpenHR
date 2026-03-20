using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.VMS.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.VMS;

public class ContingentTimeReportConfiguration : IEntityTypeConfiguration<ContingentTimeReport>
{
    public void Configure(EntityTypeBuilder<ContingentTimeReport> builder)
    {
        builder.ToTable("contingent_time_reports", "vms");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.ContingentWorkerId).HasColumnName("contingent_worker_id");
        builder.Property(e => e.Period).HasColumnName("period").HasMaxLength(20);
        builder.Property(e => e.Timmar).HasColumnName("timmar").HasColumnType("decimal(18,2)");
        builder.Property(e => e.OBTimmar).HasColumnName("ob_timmar").HasColumnType("decimal(18,2)");
        builder.Property(e => e.Overtid).HasColumnName("overtid").HasColumnType("decimal(18,2)");
        builder.Property(e => e.AtesteradAv).HasColumnName("attesterad_av");
        builder.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(30);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
    }
}
