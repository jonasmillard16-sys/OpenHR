using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Helpdesk.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Helpdesk;

public class ServiceRequestConfiguration : IEntityTypeConfiguration<ServiceRequest>
{
    public void Configure(EntityTypeBuilder<ServiceRequest> builder)
    {
        builder.ToTable("service_requests", "helpdesk");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.Titel).HasColumnName("titel").HasMaxLength(500).IsRequired();
        builder.Property(e => e.Beskrivning).HasColumnName("beskrivning").IsRequired();
        builder.Property(e => e.KategoriId).HasColumnName("kategori_id");
        builder.Property(e => e.Prioritet).HasConversion<string>().HasColumnName("prioritet").HasMaxLength(20);
        builder.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(30);
        builder.Property(e => e.KallKanal).HasColumnName("kall_kanal").HasMaxLength(50);
        builder.Property(e => e.InrapportadAv)
            .HasConversion(id => id.Value, v => EmployeeId.From(v))
            .HasColumnName("inrapportad_av");
        builder.Property(e => e.TilldeladAgent).HasColumnName("tilldelad_agent");
        builder.Property(e => e.TilldeladKo).HasColumnName("tilldelad_ko");
        builder.Property(e => e.SLADefinitionId).HasColumnName("sla_definition_id");
        builder.Property(e => e.SLADeadline).HasColumnName("sla_deadline");
        builder.Property(e => e.LostVid).HasColumnName("lost_vid");
        builder.Property(e => e.StangdVid).HasColumnName("stangd_vid");
        builder.Property(e => e.NojdhetsPoang).HasColumnName("nojdhets_poang");

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Ignore(e => e.DomainEvents);
        builder.Ignore(e => e.Version);
        builder.Ignore(e => e.CreatedBy);
        builder.Ignore(e => e.UpdatedBy);

        builder.HasMany(e => e.Kommentarer).WithOne().HasForeignKey(c => c.ServiceRequestId);
        builder.HasMany(e => e.SLAMilestones).WithOne().HasForeignKey(m => m.ServiceRequestId);

        builder.HasIndex(e => e.InrapportadAv).HasDatabaseName("ix_service_requests_inrapportad_av");
        builder.HasIndex(e => e.TilldeladAgent).HasDatabaseName("ix_service_requests_tilldelad_agent");
        builder.HasIndex(e => e.Status).HasDatabaseName("ix_service_requests_status");
    }
}
