using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Recruitment.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Recruitment;

public class CommunicationTemplateConfiguration : IEntityTypeConfiguration<CommunicationTemplate>
{
    public void Configure(EntityTypeBuilder<CommunicationTemplate> builder)
    {
        builder.ToTable("communication_templates", "recruitment");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Namn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Typ).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Amne).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Brodtext).HasMaxLength(4000).IsRequired();
    }
}

public class OnboardingChecklistConfiguration : IEntityTypeConfiguration<OnboardingChecklist>
{
    public void Configure(EntityTypeBuilder<OnboardingChecklist> builder)
    {
        builder.ToTable("onboarding_checklists", "recruitment");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.AnstallId);
        builder.HasIndex(x => x.VakansId);
        builder.OwnsMany(x => x.Items, item =>
        {
            item.ToTable("onboarding_items", "recruitment");
            item.WithOwner().HasForeignKey("OnboardingChecklistId");
            item.HasKey(x => x.Id);
            item.Property(x => x.Beskrivning).HasMaxLength(500).IsRequired();
        });
    }
}
