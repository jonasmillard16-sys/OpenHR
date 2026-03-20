using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Migration.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Migration;

public class MigrationValidationErrorConfiguration : IEntityTypeConfiguration<MigrationValidationError>
{
    public void Configure(EntityTypeBuilder<MigrationValidationError> builder)
    {
        builder.ToTable("validation_errors", "migration");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.MigrationJobId).HasColumnName("migration_job_id");
        builder.Property(e => e.RadNummer).HasColumnName("rad_nummer");
        builder.Property(e => e.Falt).HasColumnName("falt").HasMaxLength(200).IsRequired();
        builder.Property(e => e.FelTyp).HasColumnName("fel_typ").HasMaxLength(200).IsRequired();
        builder.Property(e => e.OriginalVarde).HasColumnName("original_varde").HasMaxLength(1000);
        builder.Property(e => e.ForeslagnKorrektion).HasColumnName("foreslagn_korrektion").HasMaxLength(1000);

        builder.HasIndex(e => e.MigrationJobId);
    }
}
