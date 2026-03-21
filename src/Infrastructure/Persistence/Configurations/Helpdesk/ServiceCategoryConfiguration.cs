using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Helpdesk.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Helpdesk;

public class ServiceCategoryConfiguration : IEntityTypeConfiguration<ServiceCategory>
{
    public void Configure(EntityTypeBuilder<ServiceCategory> builder)
    {
        builder.ToTable("service_categories", "helpdesk");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Namn).HasColumnName("namn").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Beskrivning).HasColumnName("beskrivning").HasMaxLength(1000);
        builder.Property(e => e.ParentId).HasColumnName("parent_id");
        builder.Property(e => e.DefaultKoId).HasColumnName("default_ko_id");
        builder.Property(e => e.DefaultPrioritet).HasConversion<string>().HasColumnName("default_prioritet").HasMaxLength(20);
        builder.Property(e => e.DefaultSLAId).HasColumnName("default_sla_id");
    }
}
