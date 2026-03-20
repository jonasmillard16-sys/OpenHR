using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Agreements.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Agreements;

public class AgreementInsurancePackageConfiguration : IEntityTypeConfiguration<AgreementInsurancePackage>
{
    public void Configure(EntityTypeBuilder<AgreementInsurancePackage> builder)
    {
        builder.ToTable("agreement_insurance_packages", "agreements");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.AvtalsId)
            .HasConversion(id => id.Value, v => CollectiveAgreementId.From(v))
            .HasColumnName("avtals_id");
        builder.Property(e => e.TGL).HasColumnName("tgl").HasColumnType("jsonb");
        builder.Property(e => e.AGS).HasColumnName("ags").HasColumnType("jsonb");
        builder.Property(e => e.TFA).HasColumnName("tfa").HasColumnType("jsonb");
        builder.Property(e => e.AFA).HasColumnName("afa").HasColumnType("jsonb");
        builder.Property(e => e.PSA).HasColumnName("psa").HasColumnType("jsonb");
    }
}
