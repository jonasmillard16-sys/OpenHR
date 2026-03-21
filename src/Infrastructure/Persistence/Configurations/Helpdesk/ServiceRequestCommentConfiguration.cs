using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Helpdesk.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Helpdesk;

public class ServiceRequestCommentConfiguration : IEntityTypeConfiguration<ServiceRequestComment>
{
    public void Configure(EntityTypeBuilder<ServiceRequestComment> builder)
    {
        builder.ToTable("service_request_comments", "helpdesk");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.ServiceRequestId).HasColumnName("service_request_id");
        builder.Property(e => e.ForfattareId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value.Value,
                v => v == null ? null : EmployeeId.From(v.Value))
            .HasColumnName("forfattare_id");
        builder.Property(e => e.Innehall).HasColumnName("innehall");
        builder.Property(e => e.ArIntern).HasColumnName("ar_intern");
        builder.Property(e => e.SkapadVid).HasColumnName("skapad_vid");
    }
}
