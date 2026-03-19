using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Infrastructure.Provisioning;

namespace RegionHR.Infrastructure.Persistence.Configurations.Provisioning;

public class ProvisioningEventConfiguration : IEntityTypeConfiguration<ProvisioningEvent>
{
    public void Configure(EntityTypeBuilder<ProvisioningEvent> builder)
    {
        builder.ToTable("provisioning_events", "provisioning");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AnstallNamn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TargetSystem).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Aktion).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Trigger).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Detaljer).HasMaxLength(2000);
        builder.HasIndex(x => x.AnstallId);
        builder.HasIndex(x => x.Tidpunkt);
    }
}

public class ProvisioningRuleConfiguration : IEntityTypeConfiguration<ProvisioningRule>
{
    public void Configure(EntityTypeBuilder<ProvisioningRule> builder)
    {
        builder.ToTable("provisioning_rules", "provisioning");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TargetSystem).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Trigger).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Aktion).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Beskrivning).HasMaxLength(500);
    }
}
