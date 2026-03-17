using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Notifications.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Notifications;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications", "notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Channel).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.ActionUrl).HasMaxLength(500);
        builder.Property(x => x.RelatedEntityType).HasMaxLength(200);
        builder.Property(x => x.RelatedEntityId).HasMaxLength(200);
        builder.HasIndex(x => new { x.UserId, x.IsRead });
        builder.HasIndex(x => x.CreatedAt);
    }
}

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("notification_templates", "notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TemplateKey).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.TemplateKey).IsUnique();
        builder.Property(x => x.TitleTemplate).HasMaxLength(500).IsRequired();
        builder.Property(x => x.MessageTemplate).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.DefaultType).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.DefaultChannel).HasConversion<string>().HasMaxLength(20);
    }
}
