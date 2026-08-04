using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Nadiano.Core.Practice;
using Nadiano.Core.Profiles;

namespace Nadiano.Web.Infrastructure.Persistence;

public class PracticeSessionRecordConfiguration : IEntityTypeConfiguration<PracticeSessionRecord>
{
    public void Configure(EntityTypeBuilder<PracticeSessionRecord> builder)
    {
        builder.ToTable("PracticeSessions");
        builder.HasKey(session => session.Id);
        builder.Property(session => session.LessonId).IsRequired().HasMaxLength(200);
        builder.Property(session => session.ContentVersion).IsRequired().HasMaxLength(50);
        builder.Property(session => session.Mode).IsRequired().HasMaxLength(20);

        builder.HasIndex(session => session.ProfileId);

        // Deleting a profile removes its practice history too — there is no
        // separate account system to reassign it to (docs/PRODUCT_CONCEPT.md §5).
        builder.HasOne<LearnerProfile>()
            .WithMany()
            .HasForeignKey(session => session.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(session => session.Attempt)
            .WithOne()
            .HasForeignKey<PracticeAttemptRecord>(attempt => attempt.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}