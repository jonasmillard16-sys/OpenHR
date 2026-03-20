using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Platform.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Platform;

public class DomainEventRecordConfiguration : IEntityTypeConfiguration<DomainEventRecord>
{
    public void Configure(EntityTypeBuilder<DomainEventRecord> builder)
    {
        builder.ToTable("domain_events", "platform");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Typ).HasColumnName("typ").HasMaxLength(200).IsRequired();
        builder.Property(x => x.AggregatTyp).HasColumnName("aggregat_typ").HasMaxLength(200).IsRequired();
        builder.Property(x => x.AggregatId).HasColumnName("aggregat_id").IsRequired();
        builder.Property(x => x.Data).HasColumnName("data").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.KorrelationsId).HasColumnName("korrelations_id").IsRequired();
        builder.Property(x => x.SkapadVid).HasColumnName("skapad_vid").IsRequired();
        builder.HasIndex(x => x.Typ);
        builder.HasIndex(x => x.AggregatId);
        builder.HasIndex(x => x.SkapadVid);
    }
}
