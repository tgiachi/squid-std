using FreeSql.DataAnnotations;

namespace SquidStd.Database.Data.Internal;

/// <summary>
/// History row recording an applied database seeder.
/// </summary>
[Table(Name = "__squidstd_seed_history")]
public sealed class SeedHistoryEntity
{
    /// <summary>The unique seeder name.</summary>
    [Column(IsPrimary = true, StringLength = 256)]
    public string Name { get; set; } = string.Empty;

    /// <summary>UTC timestamp of the successful run.</summary>
    public DateTime AppliedAt { get; set; }
}
