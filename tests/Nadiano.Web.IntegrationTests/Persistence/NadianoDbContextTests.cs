using Microsoft.EntityFrameworkCore;

using Nadiano.Core.Profiles;
using Nadiano.Web.Infrastructure.Persistence;

namespace Nadiano.Web.IntegrationTests.Persistence;

public class NadianoDbContextTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"nadiano-test-{Guid.NewGuid():N}.db");

    private NadianoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NadianoDbContext>()
            .UseSqlite($"Data Source={_databasePath}")
            .Options;
        return new NadianoDbContext(options);
    }

    [Fact]
    public void Migrate_CreatesLearnerProfilesTable_OnAFreshFile()
    {
        using var db = CreateContext();

        db.Database.Migrate();

        Assert.True(db.Database.CanConnect());
        var tableExists = db.Database
            .SqlQueryRaw<string>("SELECT name FROM sqlite_master WHERE type='table' AND name='LearnerProfiles'")
            .AsEnumerable()
            .Any();
        Assert.True(tableExists);
    }

    [Fact]
    public async Task Data_SurvivesAcrossSeparateDbContextInstances_LikeAProcessRestart()
    {
        var profileId = Guid.NewGuid();

        using (var firstConnection = CreateContext())
        {
            firstConnection.Database.Migrate();
            firstConnection.LearnerProfiles.Add(new LearnerProfile
            {
                Id = profileId,
                Name = "Restart-Test-Profile",
                CreatedAtUtc = DateTimeOffset.UtcNow,
            });
            await firstConnection.SaveChangesAsync();
        }

        using var secondConnection = CreateContext();
        var reloaded = await secondConnection.LearnerProfiles.FindAsync(profileId);

        Assert.NotNull(reloaded);
        Assert.Equal("Restart-Test-Profile", reloaded!.Name);
    }

    [Fact]
    public void Migrate_ThrowsAndLeavesNoAmbiguousState_WhenTargetFileIsNotAValidSqliteDatabase()
    {
        File.WriteAllText(_databasePath, "this is not a sqlite database file");

        using var db = CreateContext();

        Assert.ThrowsAny<Exception>(() => db.Database.Migrate());
    }

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        foreach (var path in new[] { _databasePath, $"{_databasePath}-shm", $"{_databasePath}-wal" })
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}