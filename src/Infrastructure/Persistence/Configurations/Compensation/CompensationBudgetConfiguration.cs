using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Compensation.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Compensation;

public class CompensationBudgetConfiguration : IEntityTypeConfiguration<CompensationBudget>
{
    public void Configure(EntityTypeBuilder<CompensationBudget> builder)
    {
        builder.ToTable("compensation_budgets", "compensation");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.CompensationPlanId)
            .HasConversion(id => id.Value, v => CompensationPlanId.From(v))
            .HasColumnName("compensation_plan_id");

        builder.Property(e => e.OrganizationUnitId)
            .HasConversion(id => id.Value, v => OrganizationId.From(v))
            .HasColumnName("organization_unit_id");

        builder.Property(e => e.TotalUtrymme).HasColumnName("total_utrymme").HasColumnType("numeric(18,2)");
        builder.Property(e => e.Fordelat).HasColumnName("fordelat").HasColumnType("numeric(18,2)");

        builder.Ignore(e => e.Kvar);
    }
}
