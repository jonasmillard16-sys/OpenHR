using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Positions.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Positions;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions", "positions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Titel).HasMaxLength(300).IsRequired();
        builder.Property(x => x.BESTAKod).HasMaxLength(20);
        builder.Property(x => x.AIDKod).HasMaxLength(20);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.BudgeteradManadslon).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Sysselsattningsgrad).HasColumnType("decimal(5,2)");
        builder.Property(x => x.KravdaKompetenser).HasColumnType("jsonb");
        builder.HasIndex(x => x.EnhetId);
        builder.HasIndex(x => x.InnehavareAnstallId);
        builder.OwnsMany(x => x.Historik, historik =>
        {
            historik.ToTable("position_historik", "positions");
            historik.WithOwner().HasForeignKey("PositionId");
            historik.HasKey(x => x.Id);
            historik.Property(x => x.Anledning).HasMaxLength(500);
        });
    }
}

public class HeadcountPlanConfiguration : IEntityTypeConfiguration<HeadcountPlan>
{
    public void Configure(EntityTypeBuilder<HeadcountPlan> builder)
    {
        builder.ToTable("headcount_plans", "positions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BudgeteradFTE).HasColumnType("decimal(8,2)");
        builder.Property(x => x.BudgeteradKostnad).HasColumnType("decimal(18,2)");
        builder.Property(x => x.FaktiskFTE).HasColumnType("decimal(8,2)");
        builder.Property(x => x.FaktiskKostnad).HasColumnType("decimal(18,2)");
        builder.Ignore(x => x.Avvikelse);
        builder.HasIndex(x => new { x.EnhetId, x.Ar });
    }
}
