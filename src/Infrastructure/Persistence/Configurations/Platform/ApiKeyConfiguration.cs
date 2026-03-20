using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Platform.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Platform;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("api_keys", "platform");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Namn).HasColumnName("namn").HasMaxLength(200).IsRequired();
        builder.Property(x => x.NyckelHash).HasColumnName("nyckel_hash").HasMaxLength(64).IsRequired();
        builder.Property(x => x.NyckelPrefix).HasColumnName("nyckel_prefix").HasMaxLength(8).IsRequired();
        builder.Property(x => x.Scope).HasColumnName("scope").HasColumnType("jsonb");
        builder.Property(x => x.UtgarDatum).HasColumnName("utgar_datum");
        builder.Property(x => x.SkapadAv).HasColumnName("skapad_av").HasMaxLength(200).IsRequired();
        builder.Property(x => x.SkapadVid).HasColumnName("skapad_vid").IsRequired();
        builder.Property(x => x.SenastAnvand).HasColumnName("senast_anvand");
        builder.Property(x => x.ArAktiv).HasColumnName("ar_aktiv").IsRequired();
        builder.HasIndex(x => x.NyckelHash).IsUnique();
        builder.HasIndex(x => x.ArAktiv);
    }
}
