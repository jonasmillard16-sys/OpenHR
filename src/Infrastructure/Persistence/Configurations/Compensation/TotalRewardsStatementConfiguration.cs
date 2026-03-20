using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Compensation.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Compensation;

public class TotalRewardsStatementConfiguration : IEntityTypeConfiguration<TotalRewardsStatement>
{
    public void Configure(EntityTypeBuilder<TotalRewardsStatement> builder)
    {
        builder.ToTable("total_rewards_statements", "compensation");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.AnstallId)
            .HasConversion(id => id.Value, v => EmployeeId.From(v))
            .HasColumnName("anstall_id");

        builder.Property(e => e.Ar).HasColumnName("ar");
        builder.Property(e => e.GrundLon).HasColumnName("grund_lon").HasColumnType("numeric(18,2)");
        builder.Property(e => e.Tillagg).HasColumnName("tillagg").HasColumnType("numeric(18,2)");
        builder.Property(e => e.Pension).HasColumnName("pension").HasColumnType("numeric(18,2)");
        builder.Property(e => e.Forsakringar).HasColumnName("forsakringar").HasColumnType("numeric(18,2)");
        builder.Property(e => e.Formaner).HasColumnName("formaner").HasColumnType("numeric(18,2)");
        builder.Property(e => e.AGAvgifter).HasColumnName("ag_avgifter").HasColumnType("numeric(18,2)");
        builder.Property(e => e.TotalKompensation).HasColumnName("total_kompensation").HasColumnType("numeric(18,2)");
        builder.Property(e => e.GenereradVid).HasColumnName("genererad_vid");

        builder.HasIndex(e => new { e.AnstallId, e.Ar }).IsUnique();
    }
}
