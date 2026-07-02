using SquidStd.Core.Directories;
using SquidStd.Database.Abstractions.Data.Database;
using SquidStd.Database.Services;

namespace SquidStd.Tests.Database;

public class DatabaseServiceTests
{
    [Fact]
    public async Task StartAsync_RelativeSqlitePathWithSubdirectory_CreatesDirectoryUnderRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "squidstd-root-" + Guid.NewGuid().ToString("N"));
        var service = new DatabaseService(
            new DatabaseConfig { ConnectionString = "sqlite://sub/app.db", AutoMigrate = true },
            new DirectoriesConfig(root, [])
        );

        try
        {
            await service.StartAsync();

            Assert.True(Directory.Exists(Path.Combine(root, "sub")));
        }
        finally
        {
            await service.StopAsync();

            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }
}
