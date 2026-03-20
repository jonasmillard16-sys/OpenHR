using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Compensation.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Compensation;

public class CompensationGuidelineConfiguration : IEntityTypeConfiguration<CompensationGuideline>
{
    public void Configure(EntityTypeBuilder<CompensationGuideline> builder)
    {
        builder.ToTable("compensation_guidelines", "compensation");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.CompensationPlanId)
            .HasConversion(id => id.Value, v => CompensationPlanId.From(v))
            .HasColumnName("compensation_plan_id");

        builder.Property(e => e.PrestationsNiva).HasColumnName("prestations_niva").HasMaxLength(100).IsRequired();
        builder.Property(e => e.RekommenderadHojningProcent).HasColumnName("rekommenderad_hojning_procent").HasColumnType("numeric(5,2)");
        builder.Property(e => e.MaxHojningProcent).HasColumnName("max_hojning_procent").HasColumnType("numeric(5,2)");
    }
}
