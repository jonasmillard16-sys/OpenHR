using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.LMS.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.LMS;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("courses", "lms");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Namn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Beskrivning).HasMaxLength(4000);
        builder.Property(x => x.Format).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Kategori).HasMaxLength(200);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ArObligatorisk);
    }
}

public class CourseEnrollmentConfiguration : IEntityTypeConfiguration<CourseEnrollment>
{
    public void Configure(EntityTypeBuilder<CourseEnrollment> builder)
    {
        builder.ToTable("course_enrollments", "lms");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Progress).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(x => x.AnstallId);
        builder.HasIndex(x => x.CourseId);
        builder.HasIndex(x => x.Progress);
        builder.HasIndex(x => x.GiltigTill);
    }
}

public class LearningPathConfiguration : IEntityTypeConfiguration<LearningPath>
{
    public void Configure(EntityTypeBuilder<LearningPath> builder)
    {
        builder.ToTable("learning_paths", "lms");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Namn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Beskrivning).HasMaxLength(2000);
        builder.Property(x => x.RollNamn).HasMaxLength(200);
        builder.HasMany(x => x.Steg).WithOne().HasForeignKey("LearningPathId");
    }
}

public class LearningPathStepConfiguration : IEntityTypeConfiguration<LearningPathStep>
{
    public void Configure(EntityTypeBuilder<LearningPathStep> builder)
    {
        builder.ToTable("learning_path_steps", "lms");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.CourseId);
    }
}
