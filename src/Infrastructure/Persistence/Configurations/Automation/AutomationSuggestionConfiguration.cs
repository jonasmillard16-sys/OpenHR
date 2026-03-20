using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Automation.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Automation;

public class AutomationSuggestionConfiguration : IEntityTypeConfiguration<AutomationSuggestion>
{
    public void Configure(EntityTypeBuilder<AutomationSuggestion> builder)
    {
        builder.ToTable("suggestions", "automation");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.RegelId)
            .HasConversion(id => id.Value, v => AutomationRuleId.From(v))
            .HasColumnName("regel_id");
        builder.Property(x => x.ForeslagenAtgard).HasColumnName("foreslagen_atgard").HasMaxLength(1000).IsRequired();
        builder.Property(x => x.SkapadFor)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value.Value,
                v => v == null ? null : EmployeeId.From(v.Value))
            .HasColumnName("skapad_for");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.SkapadVid).HasColumnName("skapad_vid");
        builder.Property(x => x.GiltigTill).HasColumnName("giltig_till");

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.RegelId);
    }
}
