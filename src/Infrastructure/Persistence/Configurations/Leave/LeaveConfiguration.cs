using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Leave.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Leave;

public class VacationBalanceConfiguration : IEntityTypeConfiguration<VacationBalance>
{
    public void Configure(EntityTypeBuilder<VacationBalance> builder)
    {
        builder.ToTable("vacation_balances", "leave");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.AnstallId, x.Ar }).IsUnique();
    }
}

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("leave_requests", "leave");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Typ).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Beskrivning).HasMaxLength(1000);
        builder.Property(x => x.Kommentar).HasMaxLength(1000);
        builder.HasIndex(x => x.AnstallId);
        builder.HasIndex(x => x.Status);
    }
}

public class SickLeaveNotificationConfiguration : IEntityTypeConfiguration<SickLeaveNotification>
{
    public void Configure(EntityTypeBuilder<SickLeaveNotification> builder)
    {
        builder.ToTable("sick_leave_notifications", "leave");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.AnstallId);
    }
}
