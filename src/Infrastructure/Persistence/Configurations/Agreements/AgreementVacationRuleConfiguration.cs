using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Agreements.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Agreements;

public class AgreementVacationRuleConfiguration : IEntityTypeConfiguration<AgreementVacationRule>
{
    public void Configure(EntityTypeBuilder<AgreementVacationRule> builder)
    {
        builder.ToTable("agreement_vacation_rules", "agreements");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.AvtalsId)
            .HasConversion(id => id.Value, v => CollectiveAgreementId.From(v))
            .HasColumnName("avtals_id");
        builder.Property(e => e.BasDagar).HasColumnName("bas_dagar");
        builder.Property(e => e.ExtraDagarVid40).HasColumnName("extra_dagar_vid_40");
        builder.Property(e => e.ExtraDagarVid50).HasColumnName("extra_dagar_vid_50");
    }
}
