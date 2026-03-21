using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Performance.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Performance;

public class OneOnOneMeetingConfiguration : IEntityTypeConfiguration<OneOnOneMeeting>
{
    public void Configure(EntityTypeBuilder<OneOnOneMeeting> builder)
    {
        builder.ToTable("one_on_one_meetings", "performance");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ChefId).IsRequired();
        builder.Property(x => x.AnstallId).IsRequired();
        builder.Property(x => x.Agenda).HasMaxLength(4000);
        builder.Property(x => x.Anteckningar).HasMaxLength(8000);
        builder.Property(x => x.AtgardsLista).HasColumnType("jsonb");
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(x => x.ChefId);
        builder.HasIndex(x => x.AnstallId);
        builder.HasIndex(x => x.Datum);
    }
}

public class MeetingActionItemConfiguration : IEntityTypeConfiguration<MeetingActionItem>
{
    public void Configure(EntityTypeBuilder<MeetingActionItem> builder)
    {
        builder.ToTable("meeting_action_items", "performance");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MeetingId).IsRequired();
        builder.Property(x => x.Beskrivning).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Ansvarig).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(x => x.MeetingId);
        builder.HasIndex(x => x.Ansvarig);
    }
}

public class ManagerScorecardConfiguration : IEntityTypeConfiguration<ManagerScorecard>
{
    public void Configure(EntityTypeBuilder<ManagerScorecard> builder)
    {
        builder.ToTable("manager_scorecards", "performance");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ChefId).IsRequired();
        builder.Property(x => x.Period).HasMaxLength(30).IsRequired();
        builder.Property(x => x.TeamOmsattning).HasPrecision(8, 4);
        builder.Property(x => x.EngagementDelta).HasPrecision(8, 4);
        builder.Property(x => x.UtvecklingsplanFardiggrad).HasPrecision(8, 4);
        builder.Property(x => x.MedelTidMellanOneonone).HasPrecision(8, 2);
        builder.HasIndex(x => x.ChefId);
        builder.HasIndex(x => x.Period);
    }
}

public class CoachingNudgeConfiguration : IEntityTypeConfiguration<CoachingNudge>
{
    public void Configure(EntityTypeBuilder<CoachingNudge> builder)
    {
        builder.ToTable("coaching_nudges", "performance");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ChefId).IsRequired();
        builder.Property(x => x.Typ).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Meddelande).HasMaxLength(2000).IsRequired();
        builder.HasIndex(x => x.ChefId);
        builder.HasIndex(x => x.ArLast);
    }
}
