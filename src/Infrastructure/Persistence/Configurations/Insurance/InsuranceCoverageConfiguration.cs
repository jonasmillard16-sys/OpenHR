using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Insurance.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Insurance;

public class InsuranceCoverageConfiguration : IEntityTypeConfiguration<InsuranceCoverage>
{
    public void Configure(EntityTypeBuilder<InsuranceCoverage> builder)
    {
        builder.ToTable("insurance_coverages", "insurance");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Typ).HasConversion<string>().HasColumnName("typ").HasMaxLength(20);
        builder.Property(e => e.Namn).HasColumnName("namn").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Beskrivning).HasColumnName("beskrivning").HasMaxLength(500);
        builder.Property(e => e.Forsakringsgivare).HasColumnName("forsakringsgivare").HasMaxLength(200).IsRequired();
        builder.Property(e => e.ArAktiv).HasColumnName("ar_aktiv");
        builder.Property(e => e.SkapadVid).HasColumnName("skapad_vid");
    }
}
