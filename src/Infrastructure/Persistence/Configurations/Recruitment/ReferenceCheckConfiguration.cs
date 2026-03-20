using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Recruitment.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Recruitment;

public class ReferenceCheckConfiguration : IEntityTypeConfiguration<ReferenceCheck>
{
    public void Configure(EntityTypeBuilder<ReferenceCheck> builder)
    {
        builder.ToTable("reference_checks", "recruitment");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.VacancyId).HasColumnName("vacancy_id");
        builder.Property(e => e.KandidatNamn).HasColumnName("kandidat_namn").HasMaxLength(200).IsRequired();
        builder.Property(e => e.ReferensNamn).HasColumnName("referens_namn").HasMaxLength(200).IsRequired();
        builder.Property(e => e.ReferensRelation).HasColumnName("referens_relation").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(20);
        builder.Property(e => e.Kommentar).HasColumnName("kommentar").HasMaxLength(1000);
        builder.Property(e => e.SkapadVid).HasColumnName("skapad_vid");
    }
}
