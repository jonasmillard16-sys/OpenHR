using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Scheduling.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Scheduling;

public class ScheduledShiftConfiguration : IEntityTypeConfiguration<ScheduledShift>
{
    public void Configure(EntityTypeBuilder<ScheduledShift> builder)
    {
        builder.ToTable("scheduled_shifts", "scheduling");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.SchemaId)
            .HasConversion(id => id.Value, v => ScheduleId.From(v))
            .HasColumnName("schema_id");
        builder.Property(e => e.AnstallId)
            .HasConversion(id => id.Value, v => EmployeeId.From(v))
            .HasColumnName("anstalld_id");
        builder.Property(e => e.Datum).HasColumnName("datum");
        builder.Property(e => e.PassTyp).HasConversion<string>().HasColumnName("pass_typ").HasMaxLength(20);
        builder.Property(e => e.PlaneradStart).HasColumnName("planerad_start");
        builder.Property(e => e.PlaneradSlut).HasColumnName("planerad_slut");
        builder.Property(e => e.Rast).HasColumnName("rast");
        builder.Property(e => e.FaktiskStart).HasColumnName("faktisk_start");
        builder.Property(e => e.FaktiskSlut).HasColumnName("faktisk_slut");
        builder.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(20);
        builder.Property(e => e.OBKategori).HasConversion<string>().HasColumnName("ob_kategori").HasMaxLength(20);

        builder.Ignore(e => e.PlaneradeTimmar);
        builder.Ignore(e => e.FaktiskaTimmar);
    }
}

public class TimeClockEventConfiguration : IEntityTypeConfiguration<TimeClockEvent>
{
    public void Configure(EntityTypeBuilder<TimeClockEvent> builder)
    {
        builder.ToTable("time_clock_events", "scheduling");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.AnstallId)
            .HasConversion(id => id.Value, v => EmployeeId.From(v))
            .HasColumnName("anstalld_id");
        builder.Property(e => e.Typ).HasConversion<string>().HasColumnName("typ").HasMaxLength(10);
        builder.Property(e => e.Tidpunkt).HasColumnName("tidpunkt");
        builder.Property(e => e.Kalla).HasConversion<string>().HasColumnName("kalla").HasMaxLength(20);
        builder.Property(e => e.IPAdress).HasColumnName("ip_adress").HasMaxLength(45);
        builder.Property(e => e.Latitud).HasColumnName("latitud");
        builder.Property(e => e.Longitud).HasColumnName("longitud");
        builder.Property(e => e.ArOfflineStampling).HasColumnName("ar_offline_stampling");
        builder.Property(e => e.SynkadVid).HasColumnName("synkad_vid");
        builder.Property(e => e.KopplatPassId).HasColumnName("kopplat_pass_id");
    }
}
