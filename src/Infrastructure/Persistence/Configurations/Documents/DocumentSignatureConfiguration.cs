using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Documents.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Documents;

public class DocumentSignatureConfiguration : IEntityTypeConfiguration<DocumentSignature>
{
    public void Configure(EntityTypeBuilder<DocumentSignature> builder)
    {
        builder.ToTable("document_signatures", "documents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.IPAdress).HasMaxLength(50);
        builder.HasIndex(x => x.DocumentId);
        builder.HasIndex(x => x.SignerarId);
    }
}
