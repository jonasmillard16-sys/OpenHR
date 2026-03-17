using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Core.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.CoreHR;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees", "core_hr");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, v => EmployeeId.From(v))
            .HasColumnName("id");

        // Personnummer -- stored as string for EF, encrypted at DB level via pgcrypto
        builder.Property(e => e.Personnummer)
            .HasConversion(
                p => p.ToString().Replace("-", ""),
                s => new Personnummer(s))
            .HasColumnName("personnummer_encrypted")
            .HasMaxLength(12)
            .IsRequired();

        builder.Property(e => e.Fornamn).HasColumnName("fornamn").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Efternamn).HasColumnName("efternamn").HasMaxLength(100).IsRequired();
        builder.Property(e => e.MellanNamn).HasColumnName("mellan_namn").HasMaxLength(100);
        builder.Property(e => e.Epost).HasColumnName("epost").HasMaxLength(200);
        builder.Property(e => e.Telefon).HasColumnName("telefon").HasMaxLength(30);

        // Address as owned type
        builder.OwnsOne(e => e.Adress, a =>
        {
            a.Property(x => x.Gatuadress).HasColumnName("gatuadress").HasMaxLength(200);
            a.Property(x => x.Postnummer).HasColumnName("postnummer").HasMaxLength(10);
            a.Property(x => x.Ort).HasColumnName("ort").HasMaxLength(100);
            a.Property(x => x.Land).HasColumnName("land").HasMaxLength(50);
        });

        // Bank (simplified - in production these would be encrypted BYTEA)
        builder.Property(e => e.Clearingnummer).HasColumnName("clearingnummer_encrypted").HasMaxLength(20);
        builder.Property(e => e.Kontonummer).HasColumnName("kontonummer_encrypted").HasMaxLength(30);

        // Tax
        builder.Property(e => e.Skattetabell).HasColumnName("skattetabell");
        builder.Property(e => e.Skattekolumn).HasColumnName("skattekolumn");
        builder.Property(e => e.Kommun).HasColumnName("kommun").HasMaxLength(50);
        builder.Property(e => e.KommunalSkattesats).HasColumnName("kommunal_skattesats");
        builder.Property(e => e.HarKyrkoavgift).HasColumnName("har_kyrkoavgift");
        builder.Property(e => e.Kyrkoavgiftssats).HasColumnName("kyrkoavgiftssats");
        builder.Property(e => e.HarJamkning).HasColumnName("har_jamkning");
        builder.Property(e => e.JamkningBelopp)
            .HasConversion(m => m == null ? (decimal?)null : m.Value.Amount, v => v == null ? null : Money.SEK(v.Value))
            .HasColumnName("jamkning_belopp");

        // Audit
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);

        // Navigation: Anställningar
        builder.HasMany(e => e.Anstallningar)
            .WithOne()
            .HasForeignKey(emp => emp.AnstallId);

        // Ignore domain events
        builder.Ignore(e => e.DomainEvents);
        builder.Ignore(e => e.Version);
        builder.Ignore(e => e.FulltNamn);
    }
}
