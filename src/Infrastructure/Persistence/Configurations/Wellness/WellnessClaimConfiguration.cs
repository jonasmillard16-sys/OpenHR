using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Wellness.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Wellness;

public class WellnessClaimConfiguration : IEntityTypeConfiguration<WellnessClaim>
{
    public void Configure(EntityTypeBuilder<WellnessClaim> builder)
    {
        builder.ToTable("wellness_claims", "wellness");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.AnstallId).HasColumnName("anstall_id");
        builder.Property(e => e.Aktivitet).HasColumnName("aktivitet").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Belopp).HasColumnName("belopp");
        builder.Property(e => e.Datum).HasColumnName("datum");
        builder.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(20);
        builder.Property(e => e.KvittoFilId).HasColumnName("kvitto_fil_id").HasMaxLength(100);
        builder.Property(e => e.SkapadVid).HasColumnName("skapad_vid");
        builder.Property(e => e.GodkandAv).HasColumnName("godkand_av");
        builder.Property(e => e.GodkandVid).HasColumnName("godkand_vid");
        builder.Property(e => e.Kommentar).HasColumnName("kommentar").HasMaxLength(500);
    }
}
