using SquidStd.Abstractions.Attributes;
using SquidStd.Scripting.Lua.Attributes;
using SquidStd.Workers.Attributes;

namespace SquidStd.Tests.Generators.Registration;

public class RegistrationAttributeTests
{
    [Fact]
    public void RegisterStdServiceAttribute_StoresServiceTypeAndPriority()
    {
        var attribute = new RegisterStdServiceAttribute(typeof(ISampleService)) { Priority = 25 };

        Assert.Equal(typeof(ISampleService), attribute.ServiceType);
        Assert.Equal(25, attribute.Priority);
    }

    [Fact]
    public void RegisterStdServiceAttribute_AllowsMissingServiceTypeForGeneratorDiagnostic()
    {
        var attribute = new RegisterStdServiceAttribute();

        Assert.Null(attribute.ServiceType);
        Assert.Equal(0, attribute.Priority);
    }

    [Fact]
    public void RegisterConfigSectionAttribute_StoresSectionNameAndPriority()
    {
        var attribute = new RegisterConfigSectionAttribute("workers") { Priority = -50 };

        Assert.Equal("workers", attribute.SectionName);
        Assert.Equal(-50, attribute.Priority);
    }

    [Fact]
    public void RegisterConfigSectionAttribute_AllowsMissingSectionNameForGeneratorDiagnostic()
    {
        var attribute = new RegisterConfigSectionAttribute();

        Assert.Null(attribute.SectionName);
        Assert.Equal(0, attribute.Priority);
    }

    [Fact]
    public void MarkerAttributes_CanBeConstructed()
    {
        Assert.IsType<RegisterJobHandlerAttribute>(new RegisterJobHandlerAttribute());
        Assert.IsType<RegisterScriptModuleAttribute>(new RegisterScriptModuleAttribute());
    }

    private interface ISampleService;
}
