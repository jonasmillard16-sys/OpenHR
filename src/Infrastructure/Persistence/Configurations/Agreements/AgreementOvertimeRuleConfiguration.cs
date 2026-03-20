using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Agreements.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Agreements;

public class AgreementOvertimeRuleConfiguration : IEntityTypeConfiguration<AgreementOvertimeRule>
{
    public void Configure(EntityTypeBuilder<AgreementOvertimeRule> builder)
    {
        builder.ToTable("agreement_overtime_rules", "agreements");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.AvtalsId)
            .HasConversion(id => id.Value, v => CollectiveAgreementId.From(v))
            .HasColumnName("avtals_id");
        builder.Property(e => e.Troskel).HasColumnName("troskel");
        builder.Property(e => e.Multiplikator).HasColumnName("multiplikator");
        builder.Property(e => e.MaxPerAr).HasColumnName("max_per_ar");
    }
}
