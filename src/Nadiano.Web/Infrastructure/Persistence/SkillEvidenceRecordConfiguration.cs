using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Nadiano.Core.Profiles;
using Nadiano.Core.Progress;

namespace Nadiano.Web.Infrastructure.Persistence;

public class SkillEvidenceRecordConfiguration : IEntityTypeConfiguration<SkillEvidenceRecord>
{
    public void Configure(EntityTypeBuilder<SkillEvidenceRecord> builder)
    {
        builder.ToTable("SkillEvidence");
        builder.HasKey(evidence => evidence.Id);
        builder.Property(evidence => evidence.LessonId).IsRequired().HasMaxLength(200);
        builder.Property(evidence => evidence.SkillId).IsRequired().HasMaxLength(200);

        // Used by the Lesson page to show a learner's own self-check history for a lesson.
        builder.HasIndex(evidence => new { evidence.ProfileId, evidence.LessonId });

        builder.HasOne<LearnerProfile>()
            .WithMany()
            .HasForeignKey(evidence => evidence.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
