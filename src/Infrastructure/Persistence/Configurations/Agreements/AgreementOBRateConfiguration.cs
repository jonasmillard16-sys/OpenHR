using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Agreements.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Agreements;

public class AgreementOBRateConfiguration : IEntityTypeConfiguration<AgreementOBRate>
{
    public void Configure(EntityTypeBuilder<AgreementOBRate> builder)
    {
        builder.ToTable("agreement_ob_rates", "agreements");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.AvtalsId)
            .HasConversion(id => id.Value, v => CollectiveAgreementId.From(v))
            .HasColumnName("avtals_id");
        builder.Property(e => e.Tidstyp).HasConversion<string>().HasColumnName("tidstyp").HasMaxLength(30);
        builder.Property(e => e.Belopp).HasColumnName("belopp");
        builder.Property(e => e.GiltigFran).HasColumnName("giltig_fran");
        builder.Property(e => e.GiltigTill).HasColumnName("giltig_till");
    }
}
