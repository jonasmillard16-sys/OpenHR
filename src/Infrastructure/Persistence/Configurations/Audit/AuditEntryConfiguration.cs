using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Audit.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Audit;

public class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("audit_entries", "audit");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EntityId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Action).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.OldValues).HasColumnType("jsonb");
        builder.Property(x => x.NewValues).HasColumnType("jsonb");
        builder.Property(x => x.UserId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.UserName).HasMaxLength(200);
        builder.Property(x => x.IpAddress).HasMaxLength(50);
        builder.HasIndex(x => x.EntityType);
        builder.HasIndex(x => x.Timestamp);
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
    }
}
