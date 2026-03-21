using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Helpdesk.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Helpdesk;

public class SLAMilestoneConfiguration : IEntityTypeConfiguration<SLAMilestone>
{
    public void Configure(EntityTypeBuilder<SLAMilestone> builder)
    {
        builder.ToTable("sla_milestones", "helpdesk");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.ServiceRequestId).HasColumnName("service_request_id");
        builder.Property(e => e.Typ).HasColumnName("typ").HasMaxLength(30);
        builder.Property(e => e.MalTid).HasColumnName("mal_tid");
        builder.Property(e => e.FaktiskTid).HasColumnName("faktisk_tid");
        builder.Property(e => e.ArUppfylld).HasColumnName("ar_uppfylld");
    }
}
