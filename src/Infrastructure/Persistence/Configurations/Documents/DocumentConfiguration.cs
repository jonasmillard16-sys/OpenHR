using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Documents.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Documents;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents", "documents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Kategori).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.FileName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.StoragePath).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Beskrivning).HasMaxLength(1000);
        builder.Property(x => x.UppladdadAv).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Klassificering).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(x => x.AnstallId);
        builder.HasIndex(x => x.Kategori);
        builder.HasIndex(x => x.RetentionUntil);
    }
}
