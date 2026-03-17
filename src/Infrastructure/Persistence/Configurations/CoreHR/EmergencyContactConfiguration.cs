using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Core.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.CoreHR;

public class EmergencyContactConfiguration : IEntityTypeConfiguration<EmergencyContact>
{
    public void Configure(EntityTypeBuilder<EmergencyContact> builder)
    {
        builder.ToTable("emergency_contacts", "core_hr");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Namn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Relation).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Telefon).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Epost).HasMaxLength(200);
        builder.HasIndex(x => x.AnstallId);
    }
}
