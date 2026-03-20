using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Automation.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Automation;

public class AutomationRuleConfiguration : IEntityTypeConfiguration<AutomationRule>
{
    public void Configure(EntityTypeBuilder<AutomationRule> builder)
    {
        builder.ToTable("rules", "automation");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, v => AutomationRuleId.From(v))
            .HasColumnName("id");

        builder.Property(x => x.Namn).HasColumnName("namn").HasMaxLength(200).IsRequired();
        builder.Property(x => x.KategoriId)
            .HasConversion(id => id.Value, v => AutomationCategoryId.From(v))
            .HasColumnName("kategori_id");
        builder.Property(x => x.TriggerTyp).HasColumnName("trigger_typ").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Villkor).HasColumnName("villkor").HasColumnType("jsonb");
        builder.Property(x => x.Atgard).HasColumnName("atgard").HasColumnType("jsonb");
        builder.Property(x => x.ArAktiv).HasColumnName("ar_aktiv");
        builder.Property(x => x.MinimumNiva).HasColumnName("minimum_niva").HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.ArSystemRegel).HasColumnName("ar_system_regel");

        // Audit
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);

        // Ignore domain events
        builder.Ignore(x => x.DomainEvents);
        builder.Ignore(x => x.Version);

        builder.HasIndex(x => x.KategoriId);
        builder.HasIndex(x => x.TriggerTyp);
    }
}
