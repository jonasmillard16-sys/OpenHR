using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Automation.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Automation;

public class AutomationLevelConfigConfiguration : IEntityTypeConfiguration<AutomationLevelConfig>
{
    public void Configure(EntityTypeBuilder<AutomationLevelConfig> builder)
    {
        builder.ToTable("level_configs", "automation");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.KategoriId)
            .HasConversion(id => id.Value, v => AutomationCategoryId.From(v))
            .HasColumnName("kategori_id");
        builder.Property(x => x.ValdNiva).HasColumnName("vald_niva").HasConversion<string>().HasMaxLength(20);

        // Audit
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);

        // Ignore domain events
        builder.Ignore(x => x.DomainEvents);

        builder.HasIndex(x => x.KategoriId).IsUnique();
    }
}
