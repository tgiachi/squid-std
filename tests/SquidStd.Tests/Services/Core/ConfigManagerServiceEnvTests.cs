using DryIoc;
using SquidStd.Abstractions.Extensions.Config;
using SquidStd.Core.Config;
using SquidStd.Database.Abstractions.Data.Database;
using SquidStd.Services.Core.Services;

namespace SquidStd.Tests.Services.Core;

public class ConfigManagerServiceEnvTests
{
    [Fact]
    public void Ctor_SubstitutesEnvTokensInStringProperties()
    {
        // Section binding is eager once the SquidStdConfig instance is registered into the
        // container, so the substitution is already applied by the time RegisterConfigSection
        // returns - there is no separate Load() step to trigger it anymore.
        var dir = Path.Combine(Path.GetTempPath(), "squidstd-cfg-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        Environment.SetEnvironmentVariable("SQUID_DB_PASS", "p@ss");

        try
        {
            File.WriteAllText(
                Path.Combine(dir, "app.yaml"),
                "database:\n  ConnectionString: postgres://u:$SQUID_DB_PASS@h:5432/db\n  AutoMigrate: true\n"
            );

            var container = new Container();
            var config = SquidStdConfig.Load("app", dir);
            container.RegisterInstance(config, IfAlreadyRegistered.Replace);
            container.RegisterConfigSection<DatabaseConfig>("database");
            var service = new ConfigManagerService(config, container);

            Assert.Equal("postgres://u:p@ss@h:5432/db", service.GetConfig<DatabaseConfig>().ConnectionString);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SQUID_DB_PASS", null);
            Directory.Delete(dir, true);
        }
    }
}
