using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Agreements.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Agreements;

public class PrivateCompensationPlanConfiguration : IEntityTypeConfiguration<PrivateCompensationPlan>
{
    public void Configure(EntityTypeBuilder<PrivateCompensationPlan> builder)
    {
        builder.ToTable("private_compensation_plans", "agreements");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.AvtalsId)
            .HasConversion(id => id.Value, v => CollectiveAgreementId.From(v))
            .HasColumnName("avtals_id");
        builder.Property(e => e.Bonus).HasColumnName("bonus").HasColumnType("jsonb");
        builder.Property(e => e.Provision).HasColumnName("provision").HasColumnType("jsonb");
        builder.Property(e => e.Aktier).HasColumnName("aktier").HasColumnType("jsonb");
        builder.Property(e => e.Tjanstebil).HasColumnName("tjanstebil").HasColumnType("jsonb");
    }
}
