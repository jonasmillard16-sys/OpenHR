using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Agreements.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Agreements;

public class AgreementPensionRuleConfiguration : IEntityTypeConfiguration<AgreementPensionRule>
{
    public void Configure(EntityTypeBuilder<AgreementPensionRule> builder)
    {
        builder.ToTable("agreement_pension_rules", "agreements");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.AvtalsId)
            .HasConversion(id => id.Value, v => CollectiveAgreementId.From(v))
            .HasColumnName("avtals_id");
        builder.Property(e => e.PensionsTyp).HasConversion<string>().HasColumnName("pensions_typ").HasMaxLength(30);
        builder.Property(e => e.SatsUnderTak).HasColumnName("sats_under_tak");
        builder.Property(e => e.SatsOverTak).HasColumnName("sats_over_tak");
        builder.Property(e => e.Tak).HasColumnName("tak");
        builder.Property(e => e.BerakningsModell).HasColumnName("beraknings_modell").HasColumnType("jsonb");
    }
}
