using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.PolicyManagement.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.PolicyManagement;

public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("policies", "policy");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Titel).HasColumnName("titel").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Sammanfattning).HasColumnName("sammanfattning").HasMaxLength(500);
        builder.Property(e => e.Innehall).HasColumnName("innehall").IsRequired();
        builder.Property(e => e.Kategori).HasConversion<string>().HasColumnName("kategori").HasMaxLength(30);
        builder.Property(e => e.Version).HasColumnName("version");
        builder.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(20);
        builder.Property(e => e.KraverBekraftelse).HasColumnName("kraver_bekraftelse");
        builder.Property(e => e.SkapadVid).HasColumnName("skapad_vid");
        builder.Property(e => e.PubliceradVid).HasColumnName("publicerad_vid");
        builder.Property(e => e.SkapadAv).HasColumnName("skapad_av").HasMaxLength(100);
    }
}
