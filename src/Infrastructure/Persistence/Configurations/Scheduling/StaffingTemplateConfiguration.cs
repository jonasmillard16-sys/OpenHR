using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Scheduling.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Scheduling;

public class StaffingTemplateConfiguration : IEntityTypeConfiguration<StaffingTemplate>
{
    public void Configure(EntityTypeBuilder<StaffingTemplate> builder)
    {
        builder.ToTable("staffing_templates", "scheduling");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(v => v.Value, v => StaffingTemplateId.From(v));

        builder.Property(e => e.EnhetId)
            .HasConversion(v => v.Value, v => OrganizationId.From(v));

        builder.Property(e => e.Namn).HasMaxLength(200).IsRequired();

        builder.OwnsOne(e => e.Giltighet, g =>
        {
            g.Property(d => d.Start).HasColumnName("giltighet_fran");
            g.Property(d => d.End).HasColumnName("giltighet_till");
        });

        builder.OwnsMany(e => e.Rader, r =>
        {
            r.ToTable("staffing_template_lines", "scheduling");
            r.Property(l => l.Veckodag).HasConversion<int>();
            r.Property(l => l.PassTyp).HasConversion<int>();
            r.Property(l => l.Start);
            r.Property(l => l.Slut);
            r.Property(l => l.Rast);
            r.Property(l => l.MinAntal);
            r.Property(l => l.OptimalAntal);
            r.Property(l => l.KravdaKompetenser)
                .HasColumnType("jsonb");
        });
    }
}
