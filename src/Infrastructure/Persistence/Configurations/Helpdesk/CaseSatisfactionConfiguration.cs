using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Helpdesk.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Helpdesk;

public class CaseSatisfactionConfiguration : IEntityTypeConfiguration<CaseSatisfaction>
{
    public void Configure(EntityTypeBuilder<CaseSatisfaction> builder)
    {
        builder.ToTable("case_satisfactions", "helpdesk");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.ServiceRequestId).HasColumnName("service_request_id");
        builder.Property(e => e.Poang).HasColumnName("poang");
        builder.Property(e => e.Kommentar).HasColumnName("kommentar").HasMaxLength(2000);
        builder.Property(e => e.SkapadVid).HasColumnName("skapad_vid");
    }
}
