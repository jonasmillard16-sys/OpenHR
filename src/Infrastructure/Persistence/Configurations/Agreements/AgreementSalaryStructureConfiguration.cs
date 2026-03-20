using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Agreements.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Agreements;

public class AgreementSalaryStructureConfiguration : IEntityTypeConfiguration<AgreementSalaryStructure>
{
    public void Configure(EntityTypeBuilder<AgreementSalaryStructure> builder)
    {
        builder.ToTable("agreement_salary_structures", "agreements");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.AvtalsId)
            .HasConversion(id => id.Value, v => CollectiveAgreementId.From(v))
            .HasColumnName("avtals_id");
        builder.Property(e => e.MinLonPerKategori).HasColumnName("min_lon_per_kategori").HasColumnType("jsonb");
        builder.Property(e => e.LoneSteg).HasColumnName("lone_steg").HasColumnType("jsonb");
    }
}
