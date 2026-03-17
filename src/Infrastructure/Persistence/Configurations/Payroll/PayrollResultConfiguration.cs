using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Payroll.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Payroll;

public class PayrollResultConfiguration : IEntityTypeConfiguration<PayrollResult>
{
    public void Configure(EntityTypeBuilder<PayrollResult> builder)
    {
        builder.ToTable("payroll_results", "payroll");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.KorningsId)
            .HasConversion(id => id.Value, v => PayrollRunId.From(v))
            .HasColumnName("kornings_id");
        builder.Property(e => e.AnstallId)
            .HasConversion(id => id.Value, v => EmployeeId.From(v))
            .HasColumnName("anstalld_id");
        builder.Property(e => e.AnstallningsId)
            .HasConversion(id => id.Value, v => EmploymentId.From(v))
            .HasColumnName("anstallnings_id");

        builder.Property(e => e.Year).HasColumnName("ar");
        builder.Property(e => e.Month).HasColumnName("manad");
        builder.Property(e => e.Manadslon).HasConversion(m => m.Amount, v => Money.SEK(v)).HasColumnName("manadslon");
        builder.Property(e => e.Sysselsattningsgrad).HasColumnName("sysselsattningsgrad");
        builder.Property(e => e.Kollektivavtal).HasConversion<string>().HasColumnName("kollektivavtal").HasMaxLength(10);

        builder.Property(e => e.Brutto).HasConversion(m => m.Amount, v => Money.SEK(v)).HasColumnName("brutto");
        builder.Property(e => e.SkattepliktBrutto).HasConversion(m => m.Amount, v => Money.SEK(v)).HasColumnName("skatteplikt_brutto");
        builder.Property(e => e.Skatt).HasConversion(m => m.Amount, v => Money.SEK(v)).HasColumnName("skatt");
        builder.Property(e => e.Netto).HasConversion(m => m.Amount, v => Money.SEK(v)).HasColumnName("netto");
        builder.Property(e => e.Arbetsgivaravgifter).HasConversion(m => m.Amount, v => Money.SEK(v)).HasColumnName("arbetsgivaravgifter");
        builder.Property(e => e.ArbetsgivaravgiftSats).HasColumnName("arbetsgivaravgift_sats");
        builder.Property(e => e.Semesterlon).HasConversion(m => m.Amount, v => Money.SEK(v)).HasColumnName("semesterlon");
        builder.Property(e => e.Semestertillagg).HasConversion(m => m.Amount, v => Money.SEK(v)).HasColumnName("semestertillagg");
        builder.Property(e => e.SemesterdagarIntjanade).HasColumnName("semesterdagar_intjanade");
        builder.Property(e => e.SemesterdagarUttagna).HasColumnName("semesterdagar_uttagna");
        builder.Property(e => e.Pensionsgrundande).HasConversion(m => m.Amount, v => Money.SEK(v)).HasColumnName("pensionsgrundande");
        builder.Property(e => e.Pensionsavgift).HasConversion(m => m.Amount, v => Money.SEK(v)).HasColumnName("pensionsavgift");
        builder.Property(e => e.OBTillagg).HasConversion(m => m.Amount, v => Money.SEK(v)).HasColumnName("ob_tillagg");
        builder.Property(e => e.Overtidstillagg).HasConversion(m => m.Amount, v => Money.SEK(v)).HasColumnName("overtidstillagg");
        builder.Property(e => e.Sjuklon).HasConversion(m => m.Amount, v => Money.SEK(v)).HasColumnName("sjuklon");
        builder.Property(e => e.Karensavdrag).HasConversion(m => m.Amount, v => Money.SEK(v)).HasColumnName("karensavdrag");
        builder.Property(e => e.Loneutmatning).HasConversion(m => m.Amount, v => Money.SEK(v)).HasColumnName("loneutmatning");
        builder.Property(e => e.Fackavgift).HasConversion(m => m.Amount, v => Money.SEK(v)).HasColumnName("fackavgift");
        builder.Property(e => e.OvrigaAvdrag).HasConversion(m => m.Amount, v => Money.SEK(v)).HasColumnName("ovriga_avdrag");

        builder.HasMany(e => e.Rader).WithOne().HasForeignKey("result_id");
    }
}
