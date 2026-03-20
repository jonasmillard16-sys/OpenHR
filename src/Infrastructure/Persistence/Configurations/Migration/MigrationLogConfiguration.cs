using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Migration.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Migration;

public class MigrationLogConfiguration : IEntityTypeConfiguration<MigrationLog>
{
    public void Configure(EntityTypeBuilder<MigrationLog> builder)
    {
        builder.ToTable("logs", "migration");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.MigrationJobId).HasColumnName("migration_job_id");
        builder.Property(e => e.EntityTyp).HasColumnName("entity_typ").HasMaxLength(200).IsRequired();
        builder.Property(e => e.ImporteradPostId).HasColumnName("importerad_post_id");
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(e => e.FelMeddelande).HasColumnName("fel_meddelande").HasMaxLength(2000);

        builder.HasIndex(e => e.MigrationJobId);
        builder.HasIndex(e => e.Status);
    }
}
