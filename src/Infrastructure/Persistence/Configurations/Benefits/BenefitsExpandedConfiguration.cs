using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Benefits.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Benefits;

public class EligibilityRuleConfiguration : IEntityTypeConfiguration<EligibilityRule>
{
    public void Configure(EntityTypeBuilder<EligibilityRule> builder)
    {
        builder.ToTable("eligibility_rules", "benefits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Namn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Kombination).HasMaxLength(10).IsRequired();
        builder.HasIndex(x => x.BenefitId);
        builder.HasMany(x => x.Villkor)
            .WithOne()
            .HasForeignKey(x => x.EligibilityRuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EligibilityConditionConfiguration : IEntityTypeConfiguration<EligibilityCondition>
{
    public void Configure(EntityTypeBuilder<EligibilityCondition> builder)
    {
        builder.ToTable("eligibility_conditions", "benefits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Falt).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Operator).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Varde).HasMaxLength(2000).IsRequired();
        builder.HasIndex(x => x.EligibilityRuleId);
    }
}

public class LifeEventConfiguration : IEntityTypeConfiguration<LifeEvent>
{
    public void Configure(EntityTypeBuilder<LifeEvent> builder)
    {
        builder.ToTable("life_events", "benefits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Namn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Typ).HasMaxLength(50).IsRequired();
        builder.Property(x => x.TillatnaAndringar).HasMaxLength(4000);
        builder.HasIndex(x => x.Typ);
    }
}

public class LifeEventOccurrenceConfiguration : IEntityTypeConfiguration<LifeEventOccurrence>
{
    public void Configure(EntityTypeBuilder<LifeEventOccurrence> builder)
    {
        builder.ToTable("life_event_occurrences", "benefits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired();
        builder.Property(x => x.KoppladeAtgarder).HasMaxLength(4000);
        builder.HasIndex(x => x.LifeEventId);
        builder.HasIndex(x => x.AnstallId);
        builder.HasIndex(x => x.Status);
    }
}

public class EnrollmentPeriodConfiguration : IEntityTypeConfiguration<EnrollmentPeriod>
{
    public void Configure(EntityTypeBuilder<EnrollmentPeriod> builder)
    {
        builder.ToTable("enrollment_periods", "benefits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Namn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.InkluderadePlaner).HasMaxLength(4000);
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => x.Status);
    }
}

public class BenefitEnrollmentConfiguration : IEntityTypeConfiguration<BenefitEnrollment>
{
    public void Configure(EntityTypeBuilder<BenefitEnrollment> builder)
    {
        builder.ToTable("benefit_enrollments", "benefits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.ValdNiva).HasMaxLength(200);
        builder.HasIndex(x => x.AnstallId);
        builder.HasIndex(x => x.BenefitId);
        builder.HasIndex(x => x.Status);
    }
}

public class BenefitStatementConfiguration : IEntityTypeConfiguration<BenefitStatement>
{
    public void Configure(EntityTypeBuilder<BenefitStatement> builder)
    {
        builder.ToTable("benefit_statements", "benefits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AktivaFormaner).HasMaxLength(8000);
        builder.Property(x => x.TotaltVarde).HasPrecision(18, 2);
        builder.HasIndex(x => x.AnstallId);
        builder.HasIndex(x => new { x.AnstallId, x.Ar }).IsUnique();
    }
}

public class BenefitTransactionConfiguration : IEntityTypeConfiguration<BenefitTransaction>
{
    public void Configure(EntityTypeBuilder<BenefitTransaction> builder)
    {
        builder.ToTable("benefit_transactions", "benefits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Typ).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Belopp).HasPrecision(18, 2);
        builder.Property(x => x.Beskrivning).HasMaxLength(1000);
        builder.HasIndex(x => x.AnstallId);
        builder.HasIndex(x => x.BenefitId);
        builder.HasIndex(x => x.Datum);
    }
}
