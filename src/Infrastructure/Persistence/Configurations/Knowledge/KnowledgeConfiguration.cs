using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Knowledge.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Knowledge;

public class KnowledgeCategoryConfiguration : IEntityTypeConfiguration<KnowledgeCategory>
{
    public void Configure(EntityTypeBuilder<KnowledgeCategory> builder)
    {
        builder.ToTable("categories", "knowledge");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Namn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Beskrivning).HasMaxLength(1000);
        builder.Property(x => x.Ikon).HasMaxLength(100);
        builder.HasIndex(x => x.Ordning);
    }
}

public class KnowledgeArticleConfiguration : IEntityTypeConfiguration<KnowledgeArticle>
{
    public void Configure(EntityTypeBuilder<KnowledgeArticle> builder)
    {
        builder.ToTable("articles", "knowledge");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Titel).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Innehall).IsRequired();
        builder.Property(x => x.TaggarJson).HasColumnName("taggar").HasColumnType("jsonb");
        builder.Property(x => x.HjalpsamhetPoang).HasPrecision(3, 1);
        builder.HasIndex(x => x.KategoriId);
        builder.HasIndex(x => x.ArPublicerad);
        builder.HasOne<KnowledgeCategory>().WithMany().HasForeignKey(x => x.KategoriId);
    }
}

public class ConversationSessionConfiguration : IEntityTypeConfiguration<ConversationSession>
{
    public void Configure(EntityTypeBuilder<ConversationSession> builder)
    {
        builder.ToTable("conversation_sessions", "knowledge");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.AnstallId);
        builder.HasIndex(x => x.ArAktiv);
    }
}

public class ConversationMessageConfiguration : IEntityTypeConfiguration<ConversationMessage>
{
    public void Configure(EntityTypeBuilder<ConversationMessage> builder)
    {
        builder.ToTable("conversation_messages", "knowledge");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Avsandare).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Innehall).IsRequired();
        builder.Property(x => x.UtfordAction).HasMaxLength(200);
        builder.HasIndex(x => x.SessionId);
        builder.HasOne<ConversationSession>().WithMany().HasForeignKey(x => x.SessionId);
        builder.HasOne<KnowledgeArticle>().WithMany().HasForeignKey(x => x.KallaArtikelId).IsRequired(false);
    }
}

public class AssistantActionConfiguration : IEntityTypeConfiguration<AssistantAction>
{
    public void Configure(EntityTypeBuilder<AssistantAction> builder)
    {
        builder.ToTable("assistant_actions", "knowledge");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Namn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Beskrivning).HasMaxLength(500);
        builder.Property(x => x.Route).HasMaxLength(300);
        builder.Property(x => x.ActionTyp).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.ArAktiv);
    }
}
