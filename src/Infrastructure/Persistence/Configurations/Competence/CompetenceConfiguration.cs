using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Competence.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Competence;

public class CertificationConfiguration : IEntityTypeConfiguration<Certification>
{
    public void Configure(EntityTypeBuilder<Certification> builder)
    {
        builder.ToTable("certifications", "competence");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Namn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Typ).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Utfardare).HasMaxLength(300);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(x => x.AnstallId);
        builder.HasIndex(x => x.GiltigTill);
    }
}

public class MandatoryTrainingConfiguration : IEntityTypeConfiguration<MandatoryTraining>
{
    public void Configure(EntityTypeBuilder<MandatoryTraining> builder)
    {
        builder.ToTable("mandatory_trainings", "competence");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RollNamn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.UtbildningNamn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Beskrivning).HasMaxLength(1000);
    }
}
