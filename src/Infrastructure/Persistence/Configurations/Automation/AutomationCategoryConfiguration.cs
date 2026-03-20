using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Automation.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Automation;

public class AutomationCategoryConfiguration : IEntityTypeConfiguration<AutomationCategory>
{
    public void Configure(EntityTypeBuilder<AutomationCategory> builder)
    {
        builder.ToTable("categories", "automation");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, v => AutomationCategoryId.From(v))
            .HasColumnName("id");

        builder.Property(x => x.Namn).HasColumnName("namn").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Beskrivning).HasColumnName("beskrivning").HasMaxLength(500);
        builder.Property(x => x.Ikon).HasColumnName("ikon").HasMaxLength(50);

        // Audit
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);

        // Ignore domain events
        builder.Ignore(x => x.DomainEvents);
    }
}
