using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Migration.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Migration;

public class MigrationMappingConfiguration : IEntityTypeConfiguration<MigrationMapping>
{
    public void Configure(EntityTypeBuilder<MigrationMapping> builder)
    {
        builder.ToTable("mappings", "migration");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.MigrationJobId).HasColumnName("migration_job_id");
        builder.Property(e => e.KallFalt).HasColumnName("kall_falt").HasMaxLength(200).IsRequired();
        builder.Property(e => e.MalFalt).HasColumnName("mal_falt").HasMaxLength(200).IsRequired();
        builder.Property(e => e.TransformationsRegel).HasColumnName("transformations_regel").HasMaxLength(1000);
    }
}
