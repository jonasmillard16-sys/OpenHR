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

public class KPIDefinitionConfiguration : IEntityTypeConfiguration<KPIDefinition>
{
    public void Configure(EntityTypeBuilder<KPIDefinition> builder)
    {
        builder.ToTable("kpi_definitions", "analytics");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Namn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Kategori).HasMaxLength(50).IsRequired();
        builder.Property(x => x.BerakningsFormel).HasMaxLength(2000);
        builder.Property(x => x.Enhet).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Riktning).HasMaxLength(30).IsRequired();
        builder.Property(x => x.GronTroskel).HasPrecision(18, 4);
        builder.Property(x => x.GulTroskel).HasPrecision(18, 4);
        builder.Property(x => x.RodTroskel).HasPrecision(18, 4);
        builder.HasIndex(x => x.Kategori);
        builder.HasIndex(x => x.ArAktiv);
    }
}

public class KPISnapshotConfiguration : IEntityTypeConfiguration<KPISnapshot>
{
    public void Configure(EntityTypeBuilder<KPISnapshot> builder)
    {
        builder.ToTable("kpi_snapshots", "analytics");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Period).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Varde).HasPrecision(18, 4);
        builder.Property(x => x.JamforelseVarde).HasPrecision(18, 4);
        builder.Property(x => x.Trend).HasMaxLength(20);
        builder.HasIndex(x => x.KPIDefinitionId);
        builder.HasIndex(x => x.Period);
    }
}

public class KPIAlertConfiguration : IEntityTypeConfiguration<KPIAlert>
{
    public void Configure(EntityTypeBuilder<KPIAlert> builder)
    {
        builder.ToTable("kpi_alerts", "analytics");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Troskel).HasPrecision(18, 4);
        builder.Property(x => x.Mottagare).HasMaxLength(500).IsRequired();
        builder.HasIndex(x => x.KPIDefinitionId);
    }
}

public class PredictionModelConfiguration : IEntityTypeConfiguration<PredictionModel>
{
    public void Configure(EntityTypeBuilder<PredictionModel> builder)
    {
        builder.ToTable("prediction_models", "analytics");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Namn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Typ).HasMaxLength(50).IsRequired();
        builder.Property(x => x.InputParametrar).HasColumnType("jsonb");
        builder.Property(x => x.Accuracy).HasPrecision(8, 4);
        builder.HasIndex(x => x.Typ);
    }
}

public class PredictionResultConfiguration : IEntityTypeConfiguration<PredictionResult>
{
    public void Configure(EntityTypeBuilder<PredictionResult> builder)
    {
        builder.ToTable("prediction_results", "analytics");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityTyp).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Score).HasPrecision(8, 4);
        builder.Property(x => x.RiskNiva).HasMaxLength(20).IsRequired();
        builder.Property(x => x.BidragandeFaktorer).HasColumnType("jsonb");
        builder.HasIndex(x => x.PredictionModelId);
        builder.HasIndex(x => new { x.EntityTyp, x.EntityId });
    }
}
