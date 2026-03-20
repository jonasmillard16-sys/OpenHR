using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Platform.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Platform;

public class EventDeliveryConfiguration : IEntityTypeConfiguration<EventDelivery>
{
    public void Configure(EntityTypeBuilder<EventDelivery> builder)
    {
        builder.ToTable("event_deliveries", "platform");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventSubscriptionId).HasColumnName("event_subscription_id").IsRequired();
        builder.Property(x => x.DomainEventRecordId).HasColumnName("domain_event_record_id").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.HttpStatusKod).HasColumnName("http_status_kod");
        builder.Property(x => x.AntalForsok).HasColumnName("antal_forsok");
        builder.Property(x => x.NastaRetry).HasColumnName("nasta_retry");
        builder.Property(x => x.SkapadVid).HasColumnName("skapad_vid").IsRequired();
        builder.Property(x => x.LeveradVid).HasColumnName("leverad_vid");
        builder.HasIndex(x => x.EventSubscriptionId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.NastaRetry);
    }
}
