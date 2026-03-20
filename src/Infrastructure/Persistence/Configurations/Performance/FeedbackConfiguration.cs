using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Performance.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Performance;

public class FeedbackRoundConfiguration : IEntityTypeConfiguration<FeedbackRound>
{
    public void Configure(EntityTypeBuilder<FeedbackRound> builder)
    {
        builder.ToTable("feedback_rounds", "performance");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.AnstallId).HasColumnName("anstall_id");
        builder.Property(e => e.Titel).HasColumnName("titel").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(20);
        builder.Property(e => e.SkapadVid).HasColumnName("skapad_vid");
        builder.Property(e => e.OppnadVid).HasColumnName("oppnad_vid");
        builder.Property(e => e.StangdVid).HasColumnName("stangd_vid");
    }
}

public class FeedbackResponseConfiguration : IEntityTypeConfiguration<FeedbackResponse>
{
    public void Configure(EntityTypeBuilder<FeedbackResponse> builder)
    {
        builder.ToTable("feedback_responses", "performance");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.RoundId).HasColumnName("round_id");
        builder.Property(e => e.BedomareId).HasColumnName("bedomare_id");
        builder.Property(e => e.Relation).HasColumnName("relation").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Betyg).HasColumnName("betyg");
        builder.Property(e => e.Kommentar).HasColumnName("kommentar").HasMaxLength(1000);
        builder.Property(e => e.SvaradVid).HasColumnName("svarad_vid");
    }
}
