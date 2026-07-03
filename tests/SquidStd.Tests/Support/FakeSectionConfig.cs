namespace SquidStd.Tests.Support;

/// <summary>
/// Test-only config section for bootstrap config-hook tests.
/// </summary>
public sealed class FakeSectionConfig
{
    public string Name { get; set; } = "default";

    public int Limit { get; set; } = 1;
}
