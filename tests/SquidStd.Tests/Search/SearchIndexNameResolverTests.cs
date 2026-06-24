using SquidStd.Search.Abstractions.Attributes;
using SquidStd.Search.Abstractions.Interfaces;
using SquidStd.Search.Abstractions.Search;

namespace SquidStd.Tests.Search;

public class SearchIndexNameResolverTests
{
    private sealed record PlainDoc : IIndexableEntity
    {
        public string IndexId => "1";
    }

    [SearchIndex("Orders")]
    private sealed record AttributedDoc : IIndexableEntity
    {
        public string IndexId => "1";
    }

    [SearchIndex("orders_${SQ_ENV}")]
    private sealed record EnvDoc : IIndexableEntity
    {
        public string IndexId => "1";
    }

    [SearchIndex("orders_${SQ_MISSING:-dev}")]
    private sealed record EnvDefaultDoc : IIndexableEntity
    {
        public string IndexId => "1";
    }

    [SearchIndex("orders_${SQ_REQUIRED}")]
    private sealed record EnvRequiredDoc : IIndexableEntity
    {
        public string IndexId => "1";
    }

    [Fact]
    public void Resolve_ExpandsEnvVariable()
    {
        Environment.SetEnvironmentVariable("SQ_ENV", "Prod");

        try
        {
            Assert.Equal("orders_prod", SearchIndexNameResolver.Resolve(typeof(EnvDoc)));
        }
        finally
        {
            Environment.SetEnvironmentVariable("SQ_ENV", null);
        }
    }

    [Fact]
    public void Resolve_FallsBackToLowercasedTypeName()
        => Assert.Equal("plaindoc", SearchIndexNameResolver.Resolve(typeof(PlainDoc)));

    [Fact]
    public void Resolve_Throws_WhenRequiredVariableMissing()
    {
        Environment.SetEnvironmentVariable("SQ_REQUIRED", null);
        Assert.Throws<InvalidOperationException>(() => SearchIndexNameResolver.Resolve(typeof(EnvRequiredDoc)));
    }

    [Fact]
    public void Resolve_UsesAttribute_Lowercased()
        => Assert.Equal("orders", SearchIndexNameResolver.Resolve(typeof(AttributedDoc)));

    [Fact]
    public void Resolve_UsesDefault_WhenVariableMissing()
    {
        Environment.SetEnvironmentVariable("SQ_MISSING", null);
        Assert.Equal("orders_dev", SearchIndexNameResolver.Resolve(typeof(EnvDefaultDoc)));
    }
}
