using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Compensation.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Compensation;

public class BonusPlanConfiguration : IEntityTypeConfiguration<BonusPlan>
{
    public void Configure(EntityTypeBuilder<BonusPlan> builder)
    {
        builder.ToTable("bonus_plans", "compensation");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, v => BonusPlanId.From(v))
            .HasColumnName("id");

        builder.Property(e => e.Namn).HasColumnName("namn").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Typ).HasConversion<string>().HasColumnName("typ").HasMaxLength(20);
        builder.Property(e => e.BerakningsModell).HasColumnName("beraknings_modell").HasColumnType("jsonb");
        builder.Property(e => e.UtbetalningsTidpunkt).HasColumnName("utbetalnings_tidpunkt").HasMaxLength(100);
        builder.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(20);

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.Version).HasColumnName("version");

        builder.HasMany(e => e.Targets).WithOne().HasForeignKey(t => t.BonusPlanId);

        builder.Ignore(e => e.DomainEvents);
        builder.Ignore(e => e.UpdatedAt);
        builder.Ignore(e => e.CreatedBy);
        builder.Ignore(e => e.UpdatedBy);
    }
}
