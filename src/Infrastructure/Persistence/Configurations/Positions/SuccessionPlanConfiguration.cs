using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Positions.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Positions;

public class SuccessionPlanConfiguration : IEntityTypeConfiguration<SuccessionPlan>
{
    public void Configure(EntityTypeBuilder<SuccessionPlan> builder)
    {
        builder.ToTable("succession_plans", "positions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.PositionId).HasColumnName("position_id");
        builder.Property(e => e.NuvarandeInnehavare).HasColumnName("nuvarande_innehavare");
        builder.Property(e => e.BeraknadPensionAr).HasColumnName("beraknad_pension_ar");
        builder.Property(e => e.EftertradarKandidat).HasColumnName("eftertradar_kandidat");
        builder.Property(e => e.Beredskap).HasConversion<string>().HasColumnName("beredskap").HasMaxLength(20);
        builder.Property(e => e.BeredskapProcent).HasColumnName("beredskap_procent");
        builder.Property(e => e.SkapadVid).HasColumnName("skapad_vid");
    }
}
