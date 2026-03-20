using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.CaseManagement.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.CaseManagement;

public class MBLNegotiationConfiguration : IEntityTypeConfiguration<MBLNegotiation>
{
    public void Configure(EntityTypeBuilder<MBLNegotiation> builder)
    {
        builder.ToTable("mbl_negotiations", "case_mgmt");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Arende).HasColumnName("arende").HasMaxLength(300).IsRequired();
        builder.Property(e => e.Typ).HasConversion<string>().HasColumnName("typ").HasMaxLength(20);
        builder.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(20);
        builder.Property(e => e.Datum).HasColumnName("datum");
        builder.Property(e => e.Fackombud).HasColumnName("fackombud").HasMaxLength(200);
        builder.Property(e => e.Arbetsgivarombud).HasColumnName("arbetsgivarombud").HasMaxLength(200);
        builder.Property(e => e.Protokoll).HasColumnName("protokoll");
        builder.Property(e => e.SkapadVid).HasColumnName("skapad_vid");
    }
}
