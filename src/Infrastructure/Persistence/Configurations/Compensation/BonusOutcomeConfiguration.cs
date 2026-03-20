using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Compensation.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Compensation;

public class BonusOutcomeConfiguration : IEntityTypeConfiguration<BonusOutcome>
{
    public void Configure(EntityTypeBuilder<BonusOutcome> builder)
    {
        builder.ToTable("bonus_outcomes", "compensation");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.BonusTargetId).HasColumnName("bonus_target_id");
        builder.Property(e => e.UtfallVarde).HasColumnName("utfall_varde").HasColumnType("numeric(18,2)");
        builder.Property(e => e.BeraknatBelopp).HasColumnName("beraknat_belopp").HasColumnType("numeric(18,2)");
        builder.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(20);
    }
}
