using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.GDPR.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.GDPR;

public class DataSubjectRequestConfiguration : IEntityTypeConfiguration<DataSubjectRequest>
{
    public void Configure(EntityTypeBuilder<DataSubjectRequest> builder)
    {
        builder.ToTable("data_subject_requests", "gdpr");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Typ).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.HandlaggarId).HasMaxLength(200);
        builder.Property(x => x.Kommentar).HasMaxLength(2000);
        builder.Property(x => x.ResultatFilSokvag).HasMaxLength(1000);
        builder.HasIndex(x => x.AnstallId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Deadline);
    }
}

public class RetentionRecordConfiguration : IEntityTypeConfiguration<RetentionRecord>
{
    public void Configure(EntityTypeBuilder<RetentionRecord> builder)
    {
        builder.ToTable("retention_records", "gdpr");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EntityId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.RetentionReason).HasMaxLength(500).IsRequired();
        builder.HasIndex(x => x.RetentionExpires);
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
    }
}
