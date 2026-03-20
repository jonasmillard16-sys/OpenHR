using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Agreements.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Agreements;

public class CollectiveAgreementConfiguration : IEntityTypeConfiguration<CollectiveAgreement>
{
    public void Configure(EntityTypeBuilder<CollectiveAgreement> builder)
    {
        builder.ToTable("collective_agreements", "agreements");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, v => CollectiveAgreementId.From(v))
            .HasColumnName("id");

        builder.Property(e => e.Namn).HasColumnName("namn").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Parter).HasColumnName("parter").HasMaxLength(500).IsRequired();
        builder.Property(e => e.GiltigFran).HasColumnName("giltig_fran");
        builder.Property(e => e.GiltigTill).HasColumnName("giltig_till");
        builder.Property(e => e.Bransch).HasConversion<string>().HasColumnName("bransch").HasMaxLength(50);
        builder.Property(e => e.Status).HasConversion<string>().HasColumnName("status").HasMaxLength(30);

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);

        // HasMany relationships
        builder.HasMany(e => e.OBSatser).WithOne().HasForeignKey(s => s.AvtalsId);
        builder.HasMany(e => e.OvertidsRegler).WithOne().HasForeignKey(s => s.AvtalsId);
        builder.HasMany(e => e.SemesterRegler).WithOne().HasForeignKey(s => s.AvtalsId);
        builder.HasMany(e => e.PensionsRegler).WithOne().HasForeignKey(s => s.AvtalsId);
        builder.HasMany(e => e.ViloRegler).WithOne().HasForeignKey(s => s.AvtalsId);
        builder.HasMany(e => e.ArbetstidsRegler).WithOne().HasForeignKey(s => s.AvtalsId);
        builder.HasMany(e => e.UppságningsRegler).WithOne().HasForeignKey(s => s.AvtalsId);
        builder.HasMany(e => e.ForsakringsRegler).WithOne().HasForeignKey(s => s.AvtalsId);
        builder.HasMany(e => e.LonestrukturRegler).WithOne().HasForeignKey(s => s.AvtalsId);
        builder.HasMany(e => e.PrivatErsattningsPlaner).WithOne().HasForeignKey(s => s.AvtalsId);

        builder.Ignore(e => e.DomainEvents);
        builder.Ignore(e => e.Version);
    }
}
