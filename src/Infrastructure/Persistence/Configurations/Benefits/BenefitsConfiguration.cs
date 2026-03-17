using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Benefits.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Benefits;

public class BenefitConfiguration : IEntityTypeConfiguration<Benefit>
{
    public void Configure(EntityTypeBuilder<Benefit> builder)
    {
        builder.ToTable("benefits", "benefits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Namn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Beskrivning).HasMaxLength(2000);
        builder.Property(x => x.Kategori).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.MaxBelopp).HasPrecision(18, 2);
        builder.Property(x => x.ArbetsgivarAndel).HasPrecision(5, 2);
        builder.Property(x => x.ArbetstagarAndel).HasPrecision(5, 2);
        builder.Property(x => x.EligibilityRegler).HasMaxLength(4000);
        builder.HasIndex(x => x.Kategori);
        builder.HasIndex(x => x.ArAktiv);
    }
}

public class EmployeeBenefitConfiguration : IEntityTypeConfiguration<EmployeeBenefit>
{
    public void Configure(EntityTypeBuilder<EmployeeBenefit> builder)
    {
        builder.ToTable("employee_benefits", "benefits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.ValtBelopp).HasPrecision(18, 2);
        builder.Property(x => x.LivshandardAnledning).HasMaxLength(500);
        builder.HasIndex(x => x.AnstallId);
        builder.HasIndex(x => x.BenefitId);
        builder.HasIndex(x => x.Status);
    }
}
