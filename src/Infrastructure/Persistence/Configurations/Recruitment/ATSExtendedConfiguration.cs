using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Recruitment.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Recruitment;

public class RequisitionApprovalConfiguration : IEntityTypeConfiguration<RequisitionApproval>
{
    public void Configure(EntityTypeBuilder<RequisitionApproval> builder)
    {
        builder.ToTable("requisition_approvals", "recruitment");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Kommentar).HasMaxLength(2000);
        builder.HasIndex(x => x.VakansId);
        builder.HasIndex(x => x.GodkannareId);
    }
}

public class InterviewScheduleConfiguration : IEntityTypeConfiguration<InterviewSchedule>
{
    public void Configure(EntityTypeBuilder<InterviewSchedule> builder)
    {
        builder.ToTable("interview_schedules", "recruitment");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Plats).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Anteckningar).HasMaxLength(4000);
        builder.Property(x => x.InterviewerIds).HasConversion(
            v => string.Join(",", v),
            v => v.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToList());
        builder.HasIndex(x => x.ApplicationId);
    }
}

public class ScorecardConfiguration : IEntityTypeConfiguration<Scorecard>
{
    public void Configure(EntityTypeBuilder<Scorecard> builder)
    {
        builder.ToTable("scorecards", "recruitment");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Kommentar).HasMaxLength(4000);
        builder.Property(x => x.Rekommendation).HasMaxLength(1000);
        builder.Ignore(x => x.TotalPoang);
        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.BedomareId);
    }
}

public class TalentPoolEntryConfiguration : IEntityTypeConfiguration<TalentPoolEntry>
{
    public void Configure(EntityTypeBuilder<TalentPoolEntry> builder)
    {
        builder.ToTable("talent_pool", "recruitment");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Namn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Epost).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Telefon).HasMaxLength(30);
        builder.Property(x => x.KompetensOmrade).HasMaxLength(500);
        builder.Property(x => x.Anteckningar).HasMaxLength(4000);
        builder.HasIndex(x => x.Epost);
    }
}
