using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.PolicyManagement.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.PolicyManagement;

public class PolicyConfirmationConfiguration : IEntityTypeConfiguration<PolicyConfirmation>
{
    public void Configure(EntityTypeBuilder<PolicyConfirmation> builder)
    {
        builder.ToTable("policy_confirmations", "policy");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.PolicyId).HasColumnName("policy_id");
        builder.Property(e => e.AnstallId).HasColumnName("anstall_id");
        builder.Property(e => e.PolicyVersion).HasColumnName("policy_version");
        builder.Property(e => e.BekraftadVid).HasColumnName("bekraftad_vid");

        builder.HasIndex(e => new { e.PolicyId, e.AnstallId }).IsUnique();
    }
}
