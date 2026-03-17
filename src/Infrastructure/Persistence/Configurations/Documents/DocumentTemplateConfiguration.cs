using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Documents.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Documents;

public class DocumentTemplateConfiguration : IEntityTypeConfiguration<DocumentTemplate>
{
    public void Configure(EntityTypeBuilder<DocumentTemplate> builder)
    {
        builder.ToTable("document_templates", "documents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Namn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Kategori).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.MallInnehall).HasMaxLength(10000).IsRequired();
        builder.Property(x => x.MergeFields).HasConversion(
            v => string.Join(",", v),
            v => v.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList());
        builder.HasIndex(x => x.Kategori);
    }
}
