using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Nadiano.Core.Beta;
using Nadiano.Core.Profiles;

namespace Nadiano.Web.Infrastructure.Persistence;

public sealed class LearningEvidenceRecordConfiguration : IEntityTypeConfiguration<LearningEvidenceRecord>
{
    public void Configure(EntityTypeBuilder<LearningEvidenceRecord> builder)
    {
        builder.ToTable("LearningEvidence");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.ActivityId).IsRequired().HasMaxLength(200);
        builder.Property(item => item.ActivityKind).IsRequired().HasMaxLength(50);
        builder.Property(item => item.ExpectedJson).IsRequired();
        builder.Property(item => item.ResponseJson).IsRequired();
        builder.Property(item => item.ResultJson).IsRequired();
        builder.HasIndex(item => new { item.ProfileId, item.ActivityId, item.RecordedAtUtc });
        builder.HasOne<LearnerProfile>()
            .WithMany()
            .HasForeignKey(item => item.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ReviewQueueItemConfiguration : IEntityTypeConfiguration<ReviewQueueItem>
{
    public void Configure(EntityTypeBuilder<ReviewQueueItem> builder)
    {
        builder.ToTable("ReviewQueue");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.SkillId).IsRequired().HasMaxLength(200);
        builder.Property(item => item.SourceId).IsRequired().HasMaxLength(200);
        builder.Property(item => item.ReasonCode).IsRequired().HasMaxLength(100);
        builder.HasIndex(item => new { item.ProfileId, item.SkillId, item.SourceId }).IsUnique();
        builder.HasIndex(item => new { item.ProfileId, item.DueAtUtc });
        builder.HasOne<LearnerProfile>()
            .WithMany()
            .HasForeignKey(item => item.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class PrivateLibraryItemConfiguration : IEntityTypeConfiguration<PrivateLibraryItem>
{
    public void Configure(EntityTypeBuilder<PrivateLibraryItem> builder)
    {
        builder.ToTable("PrivateLibraryItems");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.DisplayTitle).IsRequired().HasMaxLength(300);
        builder.Property(item => item.SourceFileName).IsRequired().HasMaxLength(300);
        builder.Property(item => item.StoredDirectoryName).IsRequired().HasMaxLength(100);
        builder.Property(item => item.OriginalSha256).IsRequired().HasMaxLength(64);
        builder.Property(item => item.ValidationState).IsRequired().HasMaxLength(50);
        builder.Property(item => item.WarningJson).IsRequired();
        builder.Property(item => item.MetadataJson).IsRequired();
        builder.HasIndex(item => new { item.ProfileId, item.ImportedAtUtc });
        builder.HasOne<LearnerProfile>()
            .WithMany()
            .HasForeignKey(item => item.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
