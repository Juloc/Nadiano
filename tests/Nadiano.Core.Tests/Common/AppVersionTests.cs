using Nadiano.Core.Common;

namespace Nadiano.Core.Tests.Common;

public class AppVersionTests
{
    [Fact]
    public void Current_IsNeverEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(AppVersion.Current));
    }
}