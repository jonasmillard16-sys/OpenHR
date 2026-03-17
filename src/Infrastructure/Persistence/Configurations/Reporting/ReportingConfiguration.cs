using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Reporting.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Reporting;

public class ReportDefinitionConfiguration : IEntityTypeConfiguration<ReportDefinition>
{
    public void Configure(EntityTypeBuilder<ReportDefinition> builder)
    {
        builder.ToTable("report_definitions", "reporting");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Namn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Beskrivning).HasMaxLength(1000);
        builder.Property(x => x.Typ).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.ParameterSchema).HasColumnType("jsonb");
        builder.Property(x => x.CronExpression).HasMaxLength(100);
        builder.Property(x => x.MottagareEpost).HasMaxLength(500);
    }
}

public class ReportExecutionConfiguration : IEntityTypeConfiguration<ReportExecution>
{
    public void Configure(EntityTypeBuilder<ReportExecution> builder)
    {
        builder.ToTable("report_executions", "reporting");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.ResultatFilSokvag).HasMaxLength(1000);
        builder.Property(x => x.FelMeddelande).HasMaxLength(2000);
        builder.Property(x => x.Parametrar).HasColumnType("jsonb");
        builder.HasIndex(x => x.ReportDefinitionId);
        builder.HasIndex(x => x.Status);
    }
}
