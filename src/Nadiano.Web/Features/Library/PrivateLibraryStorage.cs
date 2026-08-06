namespace Nadiano.Web.Features.Library;

public sealed class PrivateLibraryStorage
{
    public PrivateLibraryStorage(string dataPath)
    {
        RootPath = Path.Combine(dataPath, "private-library");
        StagingPath = Path.Combine(dataPath, "import-staging");
        Directory.CreateDirectory(RootPath);
        Directory.CreateDirectory(StagingPath);
    }

    public string RootPath { get; }
    public string StagingPath { get; }

    public string ItemDirectory(Guid profileId, string storedDirectoryName) =>
        Path.Combine(RootPath, profileId.ToString("N"), storedDirectoryName);
}
