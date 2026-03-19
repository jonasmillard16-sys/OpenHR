using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Pulse.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Pulse;

public class PulseSurveyConfiguration : IEntityTypeConfiguration<PulseSurvey>
{
    public void Configure(EntityTypeBuilder<PulseSurvey> builder)
    {
        builder.ToTable("pulse_surveys", "pulse");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Titel).HasColumnName("titel").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Beskrivning).HasColumnName("beskrivning").HasMaxLength(1000);
        builder.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(20);
        builder.Property(e => e.SkapadVid).HasColumnName("skapad_vid");
        builder.Property(e => e.SkapadAv).HasColumnName("skapad_av").HasMaxLength(100);
        builder.Property(e => e.OppnadVid).HasColumnName("oppnad_vid");
        builder.Property(e => e.StangdVid).HasColumnName("stangd_vid");

        builder.OwnsMany(e => e.Fragor, q =>
        {
            q.ToTable("pulse_survey_questions", "pulse");
            q.WithOwner().HasForeignKey("SurveyId");
            q.Property(x => x.Id).HasColumnName("id");
            q.Property(x => x.Text).HasColumnName("text").HasMaxLength(500).IsRequired();
            q.Property(x => x.Ordning).HasColumnName("ordning");
            q.HasKey(x => x.Id);
        });
    }
}
