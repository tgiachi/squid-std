using DryIoc;
using SquidStd.Vfs.Abstractions.Interfaces;
using SquidStd.Vfs.Extensions;
using SquidStd.Vfs.Services;

namespace SquidStd.Tests.Vfs;

public class RegisterVfsExtensionsTests
{
    [Fact]
    public void RegisterVfs_RegistersSingletonFileSystem()
    {
        using var container = new Container();
        container.RegisterVfs(_ => new InMemoryFileSystem());

        var first = container.Resolve<IVirtualFileSystem>();
        Assert.Same(first, container.Resolve<IVirtualFileSystem>());
    }
}
