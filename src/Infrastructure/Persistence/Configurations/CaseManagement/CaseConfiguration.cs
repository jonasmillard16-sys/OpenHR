using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.CaseManagement.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.CaseManagement;

public class CaseConfiguration : IEntityTypeConfiguration<Case>
{
    public void Configure(EntityTypeBuilder<Case> builder)
    {
        builder.ToTable("cases", "case_mgmt");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, v => CaseId.From(v))
            .HasColumnName("id");

        builder.Property(e => e.Typ).HasConversion<string>().HasColumnName("typ").HasMaxLength(30);
        builder.Property(e => e.AnstallId)
            .HasConversion(id => id.Value, v => EmployeeId.From(v))
            .HasColumnName("anstalld_id");
        builder.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(30);
        builder.Property(e => e.Beskrivning).HasColumnName("beskrivning");
        builder.Property(e => e.AktuellSteg).HasColumnName("aktuellt_steg").HasMaxLength(100);
        builder.Property(e => e.TilldeladTill)
            .HasConversion(id => id == null ? (Guid?)null : id.Value.Value, v => v == null ? null : EmployeeId.From(v.Value))
            .HasColumnName("tilldelad_till");
        builder.Property(e => e.SlutfordVid).HasColumnName("slutford_vid");

        // FranvaroData as JSONB
        builder.OwnsOne(e => e.FranvaroData, fd =>
        {
            fd.ToJson("franvaro_data");
        });

        builder.HasMany(e => e.Godkannanden).WithOne().HasForeignKey("case_id");
        builder.HasMany(e => e.Kommentarer).WithOne().HasForeignKey("case_id");

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Ignore(e => e.DomainEvents);
        builder.Ignore(e => e.Version);
        builder.Ignore(e => e.CreatedBy);
        builder.Ignore(e => e.UpdatedBy);
    }
}
