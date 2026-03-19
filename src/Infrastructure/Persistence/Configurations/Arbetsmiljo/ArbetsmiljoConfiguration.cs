using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Infrastructure.Arbetsmiljo;

namespace RegionHR.Infrastructure.Persistence.Configurations.Arbetsmiljo;

public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("incidents", "arbetsmiljo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RapporterareNamn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Plats).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Beskrivning).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Allvarlighetsgrad).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Typ).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.AtgardsForslag).HasMaxLength(2000);
        builder.HasIndex(x => x.EnhetId);
        builder.HasIndex(x => x.Datum);
    }
}

public class SafetyRoundConfiguration : IEntityTypeConfiguration<SafetyRound>
{
    public void Configure(EntityTypeBuilder<SafetyRound> builder)
    {
        builder.ToTable("safety_rounds", "arbetsmiljo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Deltagare).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Anteckningar).HasMaxLength(2000);
        builder.HasIndex(x => x.EnhetId);
        builder.HasIndex(x => x.Datum);
    }
}

public class RiskAssessmentConfiguration : IEntityTypeConfiguration<RiskAssessment>
{
    public void Configure(EntityTypeBuilder<RiskAssessment> builder)
    {
        builder.ToTable("risk_assessments", "arbetsmiljo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RiskNamn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Beskrivning).HasMaxLength(2000);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Atgard).HasMaxLength(2000);
        builder.Property(x => x.Ansvarig).HasMaxLength(200);
        // RiskVarde is a computed property — NOT mapped to DB
        builder.Ignore(x => x.RiskVarde);
    }
}
