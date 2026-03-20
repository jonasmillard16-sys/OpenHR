using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Core.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.CoreHR;

public class OrganizationUnitConfiguration : IEntityTypeConfiguration<OrganizationUnit>
{
    public void Configure(EntityTypeBuilder<OrganizationUnit> builder)
    {
        builder.ToTable("organization_units", "core_hr");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, v => OrganizationId.From(v))
            .HasColumnName("id");

        builder.Property(e => e.Namn).HasColumnName("namn").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Typ).HasConversion<string>().HasColumnName("typ").HasMaxLength(50);
        builder.Property(e => e.Kostnadsstalle).HasColumnName("kostnadsstalle").HasMaxLength(20).IsRequired();
        builder.Property(e => e.CFARKod).HasColumnName("cfar_kod").HasMaxLength(10);

        builder.Property(e => e.OverordnadEnhetId)
            .HasConversion(id => id == null ? (Guid?)null : id.Value.Value, v => v == null ? null : OrganizationId.From(v.Value))
            .HasColumnName("overordnad_enhet_id");

        builder.Property(e => e.ChefId)
            .HasConversion(id => id == null ? (Guid?)null : id.Value.Value, v => v == null ? null : EmployeeId.From(v.Value))
            .HasColumnName("chef_id");

        builder.Property(e => e.DefaultAvtalsId)
            .HasConversion(id => id == null ? (Guid?)null : id.Value.Value, v => v == null ? null : CollectiveAgreementId.From(v.Value))
            .HasColumnName("default_avtals_id");

        builder.OwnsOne(e => e.Giltighet, dr =>
        {
            dr.Property(x => x.Start).HasColumnName("giltig_fran");
            dr.Property(x => x.End).HasColumnName("giltig_till");
        });

        builder.HasMany(e => e.Underenheter)
            .WithOne()
            .HasForeignKey(e => e.OverordnadEnhetId);

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);

        builder.Ignore(e => e.DomainEvents);
        builder.Ignore(e => e.Version);
    }
}
