using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Agreements.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Agreements;

public class AgreementRestRuleConfiguration : IEntityTypeConfiguration<AgreementRestRule>
{
    public void Configure(EntityTypeBuilder<AgreementRestRule> builder)
    {
        builder.ToTable("agreement_rest_rules", "agreements");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.AvtalsId)
            .HasConversion(id => id.Value, v => CollectiveAgreementId.From(v))
            .HasColumnName("avtals_id");
        builder.Property(e => e.MinDygnsvila).HasColumnName("min_dygnsvila");
        builder.Property(e => e.MinVeckovila).HasColumnName("min_veckovila");
        builder.Property(e => e.RastPerPass).HasColumnName("rast_per_pass");
    }
}
