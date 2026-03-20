using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Compensation.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Compensation;

public class BonusTargetConfiguration : IEntityTypeConfiguration<BonusTarget>
{
    public void Configure(EntityTypeBuilder<BonusTarget> builder)
    {
        builder.ToTable("bonus_targets", "compensation");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.BonusPlanId)
            .HasConversion(id => id.Value, v => BonusPlanId.From(v))
            .HasColumnName("bonus_plan_id");

        builder.Property(e => e.AnstallId).HasColumnName("anstall_id");
        builder.Property(e => e.GruppId).HasColumnName("grupp_id");
        builder.Property(e => e.MalKPI).HasColumnName("mal_kpi").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Vikt).HasColumnName("vikt").HasColumnType("numeric(5,2)");
        builder.Property(e => e.Troskel).HasColumnName("troskel").HasColumnType("numeric(18,2)");
        builder.Property(e => e.Tak).HasColumnName("tak").HasColumnType("numeric(18,2)");

        builder.HasMany(e => e.Utfall).WithOne().HasForeignKey(u => u.BonusTargetId);
    }
}
