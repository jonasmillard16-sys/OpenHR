using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Agreements.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Agreements;

public class AgreementNoticePeriodConfiguration : IEntityTypeConfiguration<AgreementNoticePeriod>
{
    public void Configure(EntityTypeBuilder<AgreementNoticePeriod> builder)
    {
        builder.ToTable("agreement_notice_periods", "agreements");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.AvtalsId)
            .HasConversion(id => id.Value, v => CollectiveAgreementId.From(v))
            .HasColumnName("avtals_id");
        builder.Property(e => e.AnstallningstidManader).HasColumnName("anstallningstid_manader");
        builder.Property(e => e.UppságningstidManader).HasColumnName("uppsagningstid_manader");
    }
}
