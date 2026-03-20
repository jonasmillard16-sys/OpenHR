using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Automation.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Automation;

public class AutomationExecutionConfiguration : IEntityTypeConfiguration<AutomationExecution>
{
    public void Configure(EntityTypeBuilder<AutomationExecution> builder)
    {
        builder.ToTable("executions", "automation");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.RegelId)
            .HasConversion(id => id.Value, v => AutomationRuleId.From(v))
            .HasColumnName("regel_id");
        builder.Property(x => x.HandelseTyp).HasColumnName("handelse_typ").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Resultat).HasColumnName("resultat").HasMaxLength(500);
        builder.Property(x => x.AnvandNiva).HasColumnName("anvand_niva").HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.UtfordAtgard).HasColumnName("utford_atgard").HasMaxLength(500);
        builder.Property(x => x.Tidsstampel).HasColumnName("tidsstampel");
        builder.Property(x => x.AuditEntryId).HasColumnName("audit_entry_id");

        builder.HasIndex(x => x.RegelId);
        builder.HasIndex(x => x.Tidsstampel);
    }
}
