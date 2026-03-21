using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Analytics.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Analytics;

public class PlanningScenarioConfiguration : IEntityTypeConfiguration<PlanningScenario>
{
    public void Configure(EntityTypeBuilder<PlanningScenario> builder)
    {
        builder.ToTable("planning_scenarios", "analytics");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Namn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Beskrivning).HasMaxLength(2000);
        builder.Property(x => x.BasÅr).HasColumnName("bas_ar");
        builder.Property(x => x.Status).HasMaxLength(30).IsRequired();
        builder.Property(x => x.SkapadAv).HasMaxLength(200);
        builder.HasMany(x => x.Antaganden)
            .WithOne()
            .HasForeignKey(x => x.ScenarioId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Resultat)
            .WithOne()
            .HasForeignKey(x => x.ScenarioId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.Status);
    }
}

public class ScenarioAssumptionConfiguration : IEntityTypeConfiguration<ScenarioAssumption>
{
    public void Configure(EntityTypeBuilder<ScenarioAssumption> builder)
    {
        builder.ToTable("scenario_assumptions", "analytics");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Typ).HasMaxLength(50).IsRequired();
        builder.Property(x => x.EnhetId).HasColumnName("enhet_id");
        builder.Property(x => x.Värde).HasColumnName("varde").HasPrecision(18, 4);
        builder.Property(x => x.Beskrivning).HasMaxLength(500);
        builder.HasIndex(x => x.ScenarioId);
    }
}

public class ScenarioResultConfiguration : IEntityTypeConfiguration<ScenarioResult>
{
    public void Configure(EntityTypeBuilder<ScenarioResult> builder)
    {
        builder.ToTable("scenario_results", "analytics");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Period).HasMaxLength(20).IsRequired();
        builder.Property(x => x.FTEPrognos).HasColumnName("fte_prognos").HasPrecision(18, 4);
        builder.Property(x => x.TotalLönekostnad).HasColumnName("total_lonekostnad").HasPrecision(18, 2);
        builder.Property(x => x.AGAvgifter).HasColumnName("ag_avgifter").HasPrecision(18, 2);
        builder.Property(x => x.TotalKostnad).HasColumnName("total_kostnad").HasPrecision(18, 2);
        builder.Property(x => x.DeltaMotBudget).HasColumnName("delta_mot_budget").HasPrecision(18, 2);
        builder.Property(x => x.BeräknadVid).HasColumnName("beraknad_vid");
        builder.HasIndex(x => x.ScenarioId);
        builder.HasIndex(x => x.Period);
    }
}
