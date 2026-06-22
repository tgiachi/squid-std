namespace SquidStd.Tests.Support;

/// <summary>
/// Simple data carrier used for JSON and YAML serialization round-trip tests.
/// </summary>
public class SampleDto
{
    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the numeric count.
    /// </summary>
    public int Count { get; set; }
}
