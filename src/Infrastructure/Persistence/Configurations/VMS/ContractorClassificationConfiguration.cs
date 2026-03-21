using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.VMS.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.VMS;

public class ContractorClassificationConfiguration : IEntityTypeConfiguration<ContractorClassification>
{
    public void Configure(EntityTypeBuilder<ContractorClassification> builder)
    {
        builder.ToTable("contractor_classifications", "vms");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.ContingentWorkerId).HasColumnName("contingent_worker_id");
        builder.Property(e => e.BedömningsResultat).HasColumnName("bedomnings_resultat").HasMaxLength(30).IsRequired();
        builder.Property(e => e.RiskNivå).HasColumnName("risk_niva").HasMaxLength(20).IsRequired();
        builder.Property(e => e.Faktorer).HasColumnName("faktorer").HasColumnType("jsonb");
        builder.Property(e => e.BedömdAv).HasColumnName("bedomd_av").HasMaxLength(200);
        builder.Property(e => e.BedömdVid).HasColumnName("bedomd_vid");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(e => e.ContingentWorkerId);
        builder.HasIndex(e => e.RiskNivå);
    }
}
