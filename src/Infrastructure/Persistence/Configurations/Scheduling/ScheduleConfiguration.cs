using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Scheduling.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Scheduling;

public class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
{
    public void Configure(EntityTypeBuilder<Schedule> builder)
    {
        builder.ToTable("schedules", "scheduling");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, v => ScheduleId.From(v))
            .HasColumnName("id");

        builder.Property(e => e.EnhetId)
            .HasConversion(id => id.Value, v => OrganizationId.From(v))
            .HasColumnName("enhet_id");

        builder.Property(e => e.Namn).HasColumnName("namn").HasMaxLength(200);
        builder.Property(e => e.Typ).HasConversion<string>().HasColumnName("typ").HasMaxLength(20);
        builder.Property(e => e.CykelLangdVeckor).HasColumnName("cykel_langd_veckor");
        builder.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(20);

        builder.OwnsOne(e => e.Period, dr =>
        {
            dr.Property(x => x.Start).HasColumnName("period_start");
            dr.Property(x => x.End).HasColumnName("period_slut");
        });

        builder.HasMany(e => e.Pass).WithOne().HasForeignKey(s => s.SchemaId);

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Ignore(e => e.DomainEvents);
        builder.Ignore(e => e.Version);
        builder.Ignore(e => e.CreatedBy);
        builder.Ignore(e => e.UpdatedBy);
    }
}
