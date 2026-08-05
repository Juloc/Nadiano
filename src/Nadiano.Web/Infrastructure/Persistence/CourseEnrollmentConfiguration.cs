using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Nadiano.Core.Profiles;
using Nadiano.Core.Progress;

namespace Nadiano.Web.Infrastructure.Persistence;

public class CourseEnrollmentConfiguration : IEntityTypeConfiguration<CourseEnrollment>
{
    public void Configure(EntityTypeBuilder<CourseEnrollment> builder)
    {
        builder.ToTable("CourseEnrollments");
        builder.HasKey(enrollment => new { enrollment.ProfileId, enrollment.CourseId });
        builder.Property(enrollment => enrollment.CourseId).IsRequired().HasMaxLength(100);

        builder.HasOne<LearnerProfile>()
            .WithMany()
            .HasForeignKey(enrollment => enrollment.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}