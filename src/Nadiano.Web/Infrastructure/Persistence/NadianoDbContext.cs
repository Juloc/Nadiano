using Microsoft.EntityFrameworkCore;

using Nadiano.Core.Beta;
using Nadiano.Core.Practice;
using Nadiano.Core.Profiles;
using Nadiano.Core.Progress;

namespace Nadiano.Web.Infrastructure.Persistence;

public class NadianoDbContext(DbContextOptions<NadianoDbContext> options) : DbContext(options)
{
    public DbSet<LearnerProfile> LearnerProfiles => Set<LearnerProfile>();
    public DbSet<ProfilePreferences> ProfilePreferences => Set<ProfilePreferences>();
    public DbSet<PracticeSessionRecord> PracticeSessions => Set<PracticeSessionRecord>();
    public DbSet<PracticeAttemptRecord> PracticeAttempts => Set<PracticeAttemptRecord>();
    public DbSet<CourseEnrollment> CourseEnrollments => Set<CourseEnrollment>();
    public DbSet<LessonProgressRecord> LessonProgress => Set<LessonProgressRecord>();
    public DbSet<SkillEvidenceRecord> SkillEvidence => Set<SkillEvidenceRecord>();
    public DbSet<LearningEvidenceRecord> LearningEvidence => Set<LearningEvidenceRecord>();
    public DbSet<ReviewQueueItem> ReviewQueue => Set<ReviewQueueItem>();
    public DbSet<PrivateLibraryItem> PrivateLibraryItems => Set<PrivateLibraryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NadianoDbContext).Assembly);
    }
}
