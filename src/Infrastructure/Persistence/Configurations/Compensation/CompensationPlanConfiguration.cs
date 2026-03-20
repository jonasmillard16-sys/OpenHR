using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Compensation.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Compensation;

public class CompensationPlanConfiguration : IEntityTypeConfiguration<CompensationPlan>
{
    public void Configure(EntityTypeBuilder<CompensationPlan> builder)
    {
        builder.ToTable("compensation_plans", "compensation");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, v => CompensationPlanId.From(v))
            .HasColumnName("id");

        builder.Property(e => e.Namn).HasColumnName("namn").HasMaxLength(200).IsRequired();
        builder.Property(e => e.GiltigFran).HasColumnName("giltig_fran");
        builder.Property(e => e.GiltigTill).HasColumnName("giltig_till");
        builder.Property(e => e.TotalBudget).HasColumnName("total_budget").HasColumnType("numeric(18,2)");
        builder.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(20);

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.Version).HasColumnName("version");

        builder.HasMany(e => e.Budgetar).WithOne().HasForeignKey(b => b.CompensationPlanId);
        builder.HasMany(e => e.Riktlinjer).WithOne().HasForeignKey(r => r.CompensationPlanId);

        builder.Ignore(e => e.DomainEvents);
        builder.Ignore(e => e.UpdatedAt);
        builder.Ignore(e => e.CreatedBy);
        builder.Ignore(e => e.UpdatedBy);
    }
}
