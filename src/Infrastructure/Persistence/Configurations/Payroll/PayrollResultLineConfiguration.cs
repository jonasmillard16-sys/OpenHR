using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Payroll.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Payroll;

public class PayrollResultLineConfiguration : IEntityTypeConfiguration<PayrollResultLine>
{
    public void Configure(EntityTypeBuilder<PayrollResultLine> builder)
    {
        builder.ToTable("payroll_result_lines", "payroll");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.LoneartKod).HasColumnName("loneart_kod").HasMaxLength(10);
        builder.Property(e => e.Benamning).HasColumnName("benamning").HasMaxLength(200);
        builder.Property(e => e.Antal).HasColumnName("antal");
        builder.Property(e => e.Sats).HasConversion(m => m.Amount, v => Money.SEK(v)).HasColumnName("sats");
        builder.Property(e => e.Belopp).HasConversion(m => m.Amount, v => Money.SEK(v)).HasColumnName("belopp");
        builder.Property(e => e.Skattekategori).HasConversion<int>().HasColumnName("skattekategori");
        builder.Property(e => e.ArSemestergrundande).HasColumnName("ar_semestergrundande");
        builder.Property(e => e.ArPensionsgrundande).HasColumnName("ar_pensionsgrundande");
        builder.Property(e => e.Kostnadsstalle).HasColumnName("kostnadsstalle").HasMaxLength(20);
        builder.Property(e => e.Projekt).HasColumnName("projekt").HasMaxLength(20);
        builder.Property(e => e.AGIFaltkod).HasColumnName("agi_faltkod").HasMaxLength(10);
        builder.Property(e => e.ArAvdrag).HasColumnName("ar_avdrag");
    }
}
