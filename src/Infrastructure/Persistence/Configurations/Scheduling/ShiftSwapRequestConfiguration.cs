using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Scheduling.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Scheduling;

public class ShiftSwapRequestConfiguration : IEntityTypeConfiguration<ShiftSwapRequest>
{
    public void Configure(EntityTypeBuilder<ShiftSwapRequest> builder)
    {
        builder.ToTable("shift_swap_requests", "scheduling");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(v => v.Value, v => ShiftSwapId.From(v));

        builder.Property(e => e.BegardAv)
            .HasConversion(v => v.Value, v => EmployeeId.From(v));

        builder.Property(e => e.ErbjodsAv)
            .HasConversion(
                v => v.HasValue ? v.Value.Value : (Guid?)null,
                v => v.HasValue ? EmployeeId.From(v.Value) : null);

        builder.Property(e => e.Status).HasConversion<int>();
        builder.Property(e => e.Motivering).HasMaxLength(500);
        builder.Property(e => e.AvvisningsAnledning).HasMaxLength(500);
        builder.Property(e => e.GodkannareId).HasMaxLength(100);
        builder.Property(e => e.SkapadVid);
        builder.Property(e => e.HandlagdVid);
    }
}
