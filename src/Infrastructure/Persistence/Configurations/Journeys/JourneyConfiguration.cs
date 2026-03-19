using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Infrastructure.Journeys;

namespace RegionHR.Infrastructure.Persistence.Configurations.Journeys;

public class JourneyTemplateConfiguration : IEntityTypeConfiguration<JourneyTemplate>
{
    public void Configure(EntityTypeBuilder<JourneyTemplate> builder)
    {
        builder.ToTable("journey_templates", "journeys");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Namn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Beskrivning).HasMaxLength(2000);
        builder.Property(x => x.Kategori).HasConversion<string>().HasMaxLength(30);

        builder.OwnsMany(x => x.Steg, step =>
        {
            step.ToTable("journey_step_templates", "journeys");
            step.WithOwner().HasForeignKey("JourneyTemplateId");
            step.HasKey(s => s.Id);
            step.Property(s => s.Titel).HasMaxLength(300).IsRequired();
            step.Property(s => s.Beskrivning).HasMaxLength(2000).IsRequired();
            step.Property(s => s.AnsvarigRoll).HasMaxLength(50).IsRequired();
        });
    }
}

public class JourneyInstanceConfiguration : IEntityTypeConfiguration<JourneyInstance>
{
    public void Configure(EntityTypeBuilder<JourneyInstance> builder)
    {
        builder.ToTable("journey_instances", "journeys");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MallNamn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.AnstallNamn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(x => x.AnstallId);
        builder.HasIndex(x => x.TemplateId);
        builder.Ignore(x => x.Progress); // Computed, not stored

        builder.OwnsMany(x => x.Steg, step =>
        {
            step.ToTable("journey_step_instances", "journeys");
            step.WithOwner().HasForeignKey("JourneyInstanceId");
            step.HasKey(s => s.Id);
            step.Property(s => s.Titel).HasMaxLength(300).IsRequired();
            step.Property(s => s.Beskrivning).HasMaxLength(2000).IsRequired();
            step.Property(s => s.AnsvarigRoll).HasMaxLength(50).IsRequired();
            step.Property(s => s.KlarAv).HasMaxLength(200);
        });
    }
}
