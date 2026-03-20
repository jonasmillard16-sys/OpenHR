using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.VMS.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.VMS;

public class RateCardConfiguration : IEntityTypeConfiguration<RateCard>
{
    public void Configure(EntityTypeBuilder<RateCard> builder)
    {
        builder.ToTable("rate_cards", "vms");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.FrameworkAgreementId).HasColumnName("framework_agreement_id");
        builder.Property(e => e.YrkesKategori).HasColumnName("yrkes_kategori").HasMaxLength(200);
        builder.Property(e => e.TimPris).HasColumnName("tim_pris").HasColumnType("decimal(18,2)");
        builder.Property(e => e.OBPaslag).HasColumnName("ob_paslag").HasColumnType("decimal(18,2)");
        builder.Property(e => e.OvertidPaslag).HasColumnName("overtid_paslag").HasColumnType("decimal(18,2)");
        builder.Property(e => e.Moms).HasColumnName("moms").HasColumnType("decimal(18,2)");
    }
}
