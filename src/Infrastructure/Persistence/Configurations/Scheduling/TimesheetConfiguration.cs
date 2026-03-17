using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Scheduling.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Scheduling;

public class TimesheetConfiguration : IEntityTypeConfiguration<Timesheet>
{
    public void Configure(EntityTypeBuilder<Timesheet> builder)
    {
        builder.ToTable("timesheets", "scheduling");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.AnstallId).HasColumnName("anstalld_id");
        builder.Property(e => e.Ar).HasColumnName("ar");
        builder.Property(e => e.Manad).HasColumnName("manad");
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .HasMaxLength(20);
        builder.Property(e => e.PlaneradeTimmar).HasColumnName("planerade_timmar");
        builder.Property(e => e.FaktiskaTimmar).HasColumnName("faktiska_timmar");
        builder.Property(e => e.Overtid).HasColumnName("overtid");
        builder.Property(e => e.GodkandAv).HasColumnName("godkand_av");
        builder.Property(e => e.GodkandVid).HasColumnName("godkand_vid");
        builder.Property(e => e.SkapadVid).HasColumnName("skapad_vid");
        builder.Property(e => e.Kommentar).HasColumnName("kommentar").HasMaxLength(1000);

        builder.Ignore(e => e.Avvikelse);

        builder.HasIndex(e => new { e.AnstallId, e.Ar, e.Manad }).IsUnique();
    }
}
