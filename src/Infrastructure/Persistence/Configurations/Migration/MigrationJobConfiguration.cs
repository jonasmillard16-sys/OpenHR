using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Migration.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Migration;

public class MigrationJobConfiguration : IEntityTypeConfiguration<MigrationJob>
{
    public void Configure(EntityTypeBuilder<MigrationJob> builder)
    {
        builder.ToTable("jobs", "migration");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, v => MigrationJobId.From(v))
            .HasColumnName("id");

        builder.Property(e => e.Kalla)
            .HasConversion<string>()
            .HasColumnName("kalla")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.FilNamn)
            .HasColumnName("fil_namn")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.TotaltAntalRader)
            .HasColumnName("totalt_antal_rader");

        builder.Property(e => e.ImporteradeRader)
            .HasColumnName("importerade_rader");

        builder.Property(e => e.FelRader)
            .HasColumnName("fel_rader");

        builder.Property(e => e.SkapadAv)
            .HasColumnName("skapad_av")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.FelMeddelande)
            .HasColumnName("fel_meddelande")
            .HasMaxLength(2000);

        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);

        builder.HasMany(e => e.ValideringsFel)
            .WithOne()
            .HasForeignKey(v => v.MigrationJobId);

        builder.HasMany(e => e.Logg)
            .WithOne()
            .HasForeignKey(l => l.MigrationJobId);

        builder.HasMany(e => e.Mappningar)
            .WithOne()
            .HasForeignKey(m => m.MigrationJobId);

        builder.Ignore(e => e.DomainEvents);
        builder.Ignore(e => e.Version);

        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.CreatedAt);
    }
}
