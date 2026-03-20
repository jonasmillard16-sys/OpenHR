using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Notifications.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Notifications;

public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("notification_preferences", "notifications");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.AnstallId).HasColumnName("anstall_id");
        builder.Property(e => e.NotisTyp).HasColumnName("notis_typ").HasMaxLength(50).IsRequired();
        builder.Property(e => e.InApp).HasColumnName("in_app");
        builder.Property(e => e.Epost).HasColumnName("epost");
        builder.Property(e => e.UppdateradVid).HasColumnName("uppdaterad_vid");
        builder.HasIndex(e => new { e.AnstallId, e.NotisTyp }).IsUnique();
    }
}
