using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Payroll.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Payroll;

public class TaxTableConfiguration : IEntityTypeConfiguration<TaxTable>
{
    public void Configure(EntityTypeBuilder<TaxTable> builder)
    {
        builder.ToTable("tax_tables", "payroll");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Ar).HasColumnName("ar");
        builder.Property(e => e.Tabellnummer).HasColumnName("tabellnummer");
        builder.Property(e => e.Kolumn).HasColumnName("kolumn");

        builder.HasMany(e => e.Rader).WithOne().HasForeignKey("tax_table_id");

        builder.HasIndex(e => new { e.Ar, e.Tabellnummer, e.Kolumn }).IsUnique();
    }
}

public class TaxTableRowConfiguration : IEntityTypeConfiguration<TaxTableRow>
{
    public void Configure(EntityTypeBuilder<TaxTableRow> builder)
    {
        builder.ToTable("tax_table_rows", "payroll");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.InkomstFran).HasColumnName("inkomst_fran");
        builder.Property(e => e.InkomstTill).HasColumnName("inkomst_till");
        builder.Property(e => e.Skattebelopp).HasColumnName("skattebelopp");
    }
}

public class SalaryCodeConfiguration : IEntityTypeConfiguration<SalaryCode>
{
    public void Configure(EntityTypeBuilder<SalaryCode> builder)
    {
        builder.ToTable("salary_codes", "payroll");
        builder.HasKey(e => e.Kod);
        builder.Property(e => e.Kod).HasColumnName("kod").HasMaxLength(10);
        builder.Property(e => e.Benamning).HasColumnName("benamning").HasMaxLength(200);
        builder.Property(e => e.Skattekategori).HasConversion<string>().HasColumnName("skattekategori").HasMaxLength(30);
        builder.Property(e => e.ArSemestergrundande).HasColumnName("ar_semestergrundande");
        builder.Property(e => e.ArPensionsgrundande).HasColumnName("ar_pensionsgrundande");
        builder.Property(e => e.ArOBGrundande).HasColumnName("ar_ob_grundande");
        builder.Property(e => e.AGIFaltkod).HasColumnName("agi_faltkod").HasMaxLength(10);
        builder.Property(e => e.ArAvdrag).HasColumnName("ar_avdrag");
        builder.Property(e => e.ArAktiv).HasColumnName("ar_aktiv");
    }
}
