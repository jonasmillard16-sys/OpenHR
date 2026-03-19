using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Pulse.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Pulse;

public class PulseSurveyResponseConfiguration : IEntityTypeConfiguration<PulseSurveyResponse>
{
    public void Configure(EntityTypeBuilder<PulseSurveyResponse> builder)
    {
        builder.ToTable("pulse_survey_responses", "pulse");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.SurveyId).HasColumnName("survey_id");
        builder.Property(e => e.SvaradVid).HasColumnName("svarad_vid");

        builder.OwnsMany(e => e.Svar, a =>
        {
            a.ToTable("pulse_survey_answers", "pulse");
            a.WithOwner().HasForeignKey("ResponseId");
            a.Property(x => x.Id).HasColumnName("id");
            a.Property(x => x.FragaId).HasColumnName("fraga_id");
            a.Property(x => x.Varde).HasColumnName("varde");
            a.Property(x => x.Kommentar).HasColumnName("kommentar").HasMaxLength(500);
            a.HasKey(x => x.Id);
        });
    }
}
