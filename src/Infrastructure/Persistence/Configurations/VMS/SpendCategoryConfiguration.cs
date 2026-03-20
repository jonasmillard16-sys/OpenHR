using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.VMS.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.VMS;

public class SpendCategoryConfiguration : IEntityTypeConfiguration<SpendCategory>
{
    public void Configure(EntityTypeBuilder<SpendCategory> builder)
    {
        builder.ToTable("spend_categories", "vms");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Namn).HasColumnName("namn").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Beskrivning).HasColumnName("beskrivning").HasMaxLength(1000);
    }
}
