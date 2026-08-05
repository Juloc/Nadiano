using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Nadiano.Core.Profiles;
using Nadiano.Core.Progress;

namespace Nadiano.Web.Infrastructure.Persistence;

public class LessonProgressRecordConfiguration : IEntityTypeConfiguration<LessonProgressRecord>
{
    public void Configure(EntityTypeBuilder<LessonProgressRecord> builder)
    {
        builder.ToTable("LessonProgress");
        builder.HasKey(progress => new { progress.ProfileId, progress.LessonId });
        builder.Property(progress => progress.CourseId).IsRequired().HasMaxLength(100);
        builder.Property(progress => progress.LessonId).IsRequired().HasMaxLength(200);

        // Used by the Learn page to list a profile's completed lessons within one course.
        builder.HasIndex(progress => new { progress.ProfileId, progress.CourseId });

        builder.HasOne<LearnerProfile>()
            .WithMany()
            .HasForeignKey(progress => progress.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}