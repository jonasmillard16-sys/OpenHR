using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Infrastructure.Authorization;

namespace RegionHR.Infrastructure.Persistence.Configurations.Authorization;

public class FieldPermissionConfiguration : IEntityTypeConfiguration<FieldPermission>
{
    public void Configure(EntityTypeBuilder<FieldPermission> builder)
    {
        builder.ToTable("field_permissions", "authorization");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Roll).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FieldName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.AccessLevel).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(x => x.Roll);
        builder.HasIndex(x => new { x.Roll, x.EntityType });
    }
}

public class DelegatedAccessConfiguration : IEntityTypeConfiguration<DelegatedAccess>
{
    public void Configure(EntityTypeBuilder<DelegatedAccess> builder)
    {
        builder.ToTable("delegated_access", "authorization");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Roll).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Anledning).HasMaxLength(500);
        builder.HasIndex(x => x.DelegatorId);
        builder.HasIndex(x => x.DelegatId);
        builder.HasIndex(x => new { x.DelegatId, x.ArAktiv });
    }
}
