using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Helpdesk.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Helpdesk;

public class CaseTemplateConfiguration : IEntityTypeConfiguration<CaseTemplate>
{
    public void Configure(EntityTypeBuilder<CaseTemplate> builder)
    {
        builder.ToTable("case_templates", "helpdesk");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Namn).HasColumnName("namn").HasMaxLength(200).IsRequired();
        builder.Property(e => e.KategoriId).HasColumnName("kategori_id");
        builder.Property(e => e.MallInnehall).HasColumnName("mall_innehall");
        builder.Property(e => e.Checklista).HasColumnName("checklista")
            .HasColumnType("jsonb");
    }
}
