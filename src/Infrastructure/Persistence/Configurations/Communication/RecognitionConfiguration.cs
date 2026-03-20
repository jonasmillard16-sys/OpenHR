using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Communication.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Communication;

public class RecognitionConfiguration : IEntityTypeConfiguration<Recognition>
{
    public void Configure(EntityTypeBuilder<Recognition> builder)
    {
        builder.ToTable("recognitions", "communication");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.FranAnstallId).HasColumnName("fran_anstall_id");
        builder.Property(e => e.TillAnstallId).HasColumnName("till_anstall_id");
        builder.Property(e => e.Kategori).HasColumnName("kategori").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Meddelande).HasColumnName("meddelande").HasMaxLength(1000).IsRequired();
        builder.Property(e => e.SkapadVid).HasColumnName("skapad_vid");
    }
}
