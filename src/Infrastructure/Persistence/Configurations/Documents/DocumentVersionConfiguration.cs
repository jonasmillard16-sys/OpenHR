using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Documents.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Documents;

public class DocumentVersionConfiguration : IEntityTypeConfiguration<DocumentVersion>
{
    public void Configure(EntityTypeBuilder<DocumentVersion> builder)
    {
        builder.ToTable("document_versions", "documents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StoragePath).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.SkapadAv).HasMaxLength(200).IsRequired();
        builder.Property(x => x.AndringsBeskrivning).HasMaxLength(1000);
        builder.HasIndex(x => x.DocumentId);
        builder.HasIndex(x => new { x.DocumentId, x.VersionNummer }).IsUnique();
    }
}
