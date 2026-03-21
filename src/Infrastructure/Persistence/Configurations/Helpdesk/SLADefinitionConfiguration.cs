using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Helpdesk.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Helpdesk;

public class SLADefinitionConfiguration : IEntityTypeConfiguration<SLADefinition>
{
    public void Configure(EntityTypeBuilder<SLADefinition> builder)
    {
        builder.ToTable("sla_definitions", "helpdesk");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Namn).HasColumnName("namn").HasMaxLength(200).IsRequired();
        builder.Property(e => e.ForsvarstidMinuter).HasColumnName("forsvarstid_minuter");
        builder.Property(e => e.LostidMinuter).HasColumnName("lostid_minuter");
        builder.Property(e => e.EskaleringEfterMinuter).HasColumnName("eskalering_efter_minuter");
        builder.Property(e => e.ArAktiv).HasColumnName("ar_aktiv");

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Ignore(e => e.DomainEvents);
        builder.Ignore(e => e.Version);
        builder.Ignore(e => e.CreatedBy);
        builder.Ignore(e => e.UpdatedBy);
    }
}
