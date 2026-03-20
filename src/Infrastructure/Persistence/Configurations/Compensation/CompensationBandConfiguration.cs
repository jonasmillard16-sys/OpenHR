using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Compensation.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Compensation;

public class CompensationBandConfiguration : IEntityTypeConfiguration<CompensationBand>
{
    public void Configure(EntityTypeBuilder<CompensationBand> builder)
    {
        builder.ToTable("compensation_bands", "compensation");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.Befattningskategori).HasColumnName("befattningskategori").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Min).HasColumnName("min_lon").HasColumnType("numeric(18,2)");
        builder.Property(e => e.Mal).HasColumnName("mal_lon").HasColumnType("numeric(18,2)");
        builder.Property(e => e.Max).HasColumnName("max_lon").HasColumnType("numeric(18,2)");

        builder.Property(e => e.Steg1Min).HasColumnName("steg1_min").HasColumnType("numeric(18,2)");
        builder.Property(e => e.Steg1Max).HasColumnName("steg1_max").HasColumnType("numeric(18,2)");
        builder.Property(e => e.Steg2Min).HasColumnName("steg2_min").HasColumnType("numeric(18,2)");
        builder.Property(e => e.Steg2Max).HasColumnName("steg2_max").HasColumnType("numeric(18,2)");
        builder.Property(e => e.Steg3Min).HasColumnName("steg3_min").HasColumnType("numeric(18,2)");
        builder.Property(e => e.Steg3Max).HasColumnName("steg3_max").HasColumnType("numeric(18,2)");
        builder.Property(e => e.Steg4Min).HasColumnName("steg4_min").HasColumnType("numeric(18,2)");
        builder.Property(e => e.Steg4Max).HasColumnName("steg4_max").HasColumnType("numeric(18,2)");
    }
}
