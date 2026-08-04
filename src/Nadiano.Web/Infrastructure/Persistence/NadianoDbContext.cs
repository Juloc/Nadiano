using Microsoft.EntityFrameworkCore;

using Nadiano.Core.Practice;
using Nadiano.Core.Profiles;

namespace Nadiano.Web.Infrastructure.Persistence;

public class NadianoDbContext(DbContextOptions<NadianoDbContext> options) : DbContext(options)
{
    public DbSet<LearnerProfile> LearnerProfiles => Set<LearnerProfile>();
    public DbSet<ProfilePreferences> ProfilePreferences => Set<ProfilePreferences>();
    public DbSet<PracticeSessionRecord> PracticeSessions => Set<PracticeSessionRecord>();
    public DbSet<PracticeAttemptRecord> PracticeAttempts => Set<PracticeAttemptRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NadianoDbContext).Assembly);
    }
}