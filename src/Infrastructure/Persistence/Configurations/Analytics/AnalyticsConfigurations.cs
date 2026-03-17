using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Analytics.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Analytics;

public class SavedReportConfiguration : IEntityTypeConfiguration<SavedReport>
{
    public void Configure(EntityTypeBuilder<SavedReport> builder)
    {
        builder.ToTable("saved_reports", "analytics");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Namn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Beskrivning).HasMaxLength(1000);
        builder.Property(x => x.QueryDefinition).HasColumnType("jsonb");
        builder.Property(x => x.Visualisering).HasMaxLength(30);
        builder.HasIndex(x => x.SkapadAvId);
        builder.HasIndex(x => x.ArDelad);
    }
}

public class DashboardConfiguration : IEntityTypeConfiguration<Dashboard>
{
    public void Configure(EntityTypeBuilder<Dashboard> builder)
    {
        builder.ToTable("dashboards", "analytics");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Namn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Layout).HasColumnType("jsonb");
        builder.HasIndex(x => x.AgarId);
        builder.HasIndex(x => x.ArDelad);
    }
}
