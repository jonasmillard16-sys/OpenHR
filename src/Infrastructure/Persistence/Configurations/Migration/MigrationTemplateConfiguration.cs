using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Migration.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Migration;

public class MigrationTemplateConfiguration : IEntityTypeConfiguration<MigrationTemplate>
{
    public void Configure(EntityTypeBuilder<MigrationTemplate> builder)
    {
        builder.ToTable("templates", "migration");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Namn).HasColumnName("namn").HasMaxLength(200).IsRequired();
        builder.Property(e => e.KallSystem)
            .HasConversion<string>()
            .HasColumnName("kall_system")
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(e => e.Mappningar).HasColumnName("mappningar").HasColumnType("jsonb");
    }
}
