using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Communication.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Communication;

public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
{
    public void Configure(EntityTypeBuilder<Announcement> builder)
    {
        builder.ToTable("announcements", "communication");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Titel).HasColumnName("titel").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Innehall).HasColumnName("innehall").IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(20);
        builder.Property(e => e.Prioritet).HasConversion<string>().HasColumnName("prioritet").HasMaxLength(20);
        builder.Property(e => e.SkapadVid).HasColumnName("skapad_vid");
        builder.Property(e => e.SkapadAv).HasColumnName("skapad_av").HasMaxLength(100);
        builder.Property(e => e.PubliceradVid).HasColumnName("publicerad_vid");
    }
}
