using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Agreements.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Agreements;

public class AgreementWorkingHoursConfiguration : IEntityTypeConfiguration<AgreementWorkingHours>
{
    public void Configure(EntityTypeBuilder<AgreementWorkingHours> builder)
    {
        builder.ToTable("agreement_working_hours", "agreements");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.AvtalsId)
            .HasConversion(id => id.Value, v => CollectiveAgreementId.From(v))
            .HasColumnName("avtals_id");
        builder.Property(e => e.NormalTimmarPerVecka).HasColumnName("normal_timmar_per_vecka");
        builder.Property(e => e.FlexRegler).HasColumnName("flex_regler").HasColumnType("jsonb");
    }
}
