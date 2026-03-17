using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Offboarding.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Offboarding;

public class OffboardingConfiguration : IEntityTypeConfiguration<OffboardingCase>
{
    public void Configure(EntityTypeBuilder<OffboardingCase> builder)
    {
        builder.ToTable("offboarding_cases", "offboarding");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Anledning).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.ExitSamtalKommentar).HasMaxLength(4000);
        builder.Property(x => x.ReHireKommentar).HasMaxLength(2000);
        builder.HasIndex(x => x.AnstallId);
        builder.OwnsMany(x => x.Steg, steg =>
        {
            steg.ToTable("offboarding_items", "offboarding");
            steg.WithOwner().HasForeignKey("OffboardingCaseId");
            steg.HasKey(x => x.Id);
            steg.Property(x => x.Beskrivning).HasMaxLength(500).IsRequired();
            steg.Property(x => x.Kommentar).HasMaxLength(2000);
        });
    }
}
