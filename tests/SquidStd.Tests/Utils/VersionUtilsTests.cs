using SquidStd.Core.Utils;

namespace SquidStd.Tests.Utils;

public class VersionUtilsTests
{
    [Fact]
    public void GetVersion_CoreAssembly_ReturnsDeclaredVersionWithoutMetadata()
    {
        var version = VersionUtils.GetVersion();

        Assert.False(string.IsNullOrWhiteSpace(version));
        Assert.DoesNotContain('+', version);
        Assert.StartsWith("0.1.0", version);
    }

    [Fact]
    public void GetVersion_ExplicitAssembly_ReturnsNonEmptyVersion()
    {
        var version = VersionUtils.GetVersion(typeof(VersionUtils).Assembly);

        Assert.False(string.IsNullOrWhiteSpace(version));
    }

    [Fact]
    public void GetVersion_NullAssembly_Throws()
        => Assert.Throws<ArgumentNullException>(() => VersionUtils.GetVersion(null!));
}
