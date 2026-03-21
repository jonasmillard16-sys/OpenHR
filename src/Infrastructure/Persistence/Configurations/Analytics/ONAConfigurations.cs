using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Analytics.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Analytics;

public class ONASurveyConfiguration : IEntityTypeConfiguration<ONASurvey>
{
    public void Configure(EntityTypeBuilder<ONASurvey> builder)
    {
        builder.ToTable("ona_surveys", "analytics");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Namn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Period).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Fragor).HasColumnType("jsonb");
        builder.HasIndex(x => x.Status);
    }
}

public class ONAResponseConfiguration : IEntityTypeConfiguration<ONAResponse>
{
    public void Configure(EntityTypeBuilder<ONAResponse> builder)
    {
        builder.ToTable("ona_responses", "analytics");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SurveyId).IsRequired();
        builder.Property(x => x.RespondentId).IsRequired();
        builder.Property(x => x.NomineradId).IsRequired();
        builder.HasIndex(x => x.SurveyId);
        builder.HasIndex(x => x.RespondentId);
        builder.HasIndex(x => x.NomineradId);
    }
}

public class NetworkNodeConfiguration : IEntityTypeConfiguration<NetworkNode>
{
    public void Configure(EntityTypeBuilder<NetworkNode> builder)
    {
        builder.ToTable("network_nodes", "analytics");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SurveyId).IsRequired();
        builder.Property(x => x.AnstallId).IsRequired();
        builder.Property(x => x.BetweennessCentrality).HasPrecision(12, 6);
        builder.Property(x => x.Kluster).HasMaxLength(100);
        builder.Property(x => x.Roll).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.SurveyId);
        builder.HasIndex(x => x.AnstallId);
    }
}

public class NetworkEdgeConfiguration : IEntityTypeConfiguration<NetworkEdge>
{
    public void Configure(EntityTypeBuilder<NetworkEdge> builder)
    {
        builder.ToTable("network_edges", "analytics");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SurveyId).IsRequired();
        builder.Property(x => x.FranAnstallId).IsRequired();
        builder.Property(x => x.TillAnstallId).IsRequired();
        builder.Property(x => x.Styrka).HasPrecision(8, 4);
        builder.HasIndex(x => x.SurveyId);
        builder.HasIndex(x => new { x.FranAnstallId, x.TillAnstallId });
    }
}
