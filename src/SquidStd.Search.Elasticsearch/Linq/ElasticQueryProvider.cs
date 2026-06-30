using System.Linq.Expressions;
using System.Text.Json.Nodes;
using SquidStd.Search.Elasticsearch.Services;
using HttpMethod = Elastic.Transport.HttpMethod;

namespace SquidStd.Search.Elasticsearch.Linq;

/// <summary>
/// Executes a translated <see cref="ElasticQuery" /> against Elasticsearch. Synchronous LINQ execution is not
/// supported — use the async terminals in <see cref="ElasticQueryableExtensions" />.
/// </summary>
public sealed class ElasticQueryProvider : IQueryProvider
{
    private readonly Type _elementType;
    private readonly string _index;
    private readonly ElasticTransport _transport;

    public ElasticQueryProvider(ElasticTransport transport, string index, Type elementType)
    {
        _transport = transport;
        _index = index;
        _elementType = elementType;
    }

    public IQueryable CreateQuery(Expression expression)
        => (IQueryable)Activator.CreateInstance(
            typeof(ElasticQueryable<>).MakeGenericType(_elementType),
            this,
            expression
        )!;

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => new ElasticQueryable<TElement>(this, expression);

    public object? Execute(Expression expression)
        => throw new NotSupportedException("Use the async terminals (ToListAsync/CountAsync/FirstOrDefaultAsync).");

    public TResult Execute<TResult>(Expression expression)
        => throw new NotSupportedException("Use the async terminals (ToListAsync/CountAsync/FirstOrDefaultAsync).");

    /// <summary>Runs a count and returns the total.</summary>
    public async Task<long> CountAsync(Expression expression, CancellationToken cancellationToken)
    {
        var query = ElasticExpressionTranslator.Translate(expression, _elementType);
        var body = new JsonObject { ["query"] = query.Query.DeepClone() };
        var (status, response) = await _transport.SendAsync(HttpMethod.POST, $"/{_index}/_count", body, cancellationToken);

        return status == 404 ? 0 : response?["count"]?.GetValue<long>() ?? 0;
    }

    /// <summary>Runs the search and returns the deserialized hits.</summary>
    public async Task<List<T>> ToListAsync<T>(Expression expression, CancellationToken cancellationToken)
    {
        var query = ElasticExpressionTranslator.Translate(expression, _elementType);
        var (status, body) = await _transport.SendAsync(
                                 HttpMethod.POST,
                                 $"/{_index}/_search",
                                 query.ToRequestBody(),
                                 cancellationToken
                             );

        if (status == 404)
        {
            return [];
        }

        var hits = body?["hits"]?["hits"]?.AsArray();
        var results = new List<T>();

        if (hits is not null)
        {
            foreach (var hit in hits)
            {
                var source = hit?["_source"];

                if (source is not null)
                {
                    results.Add(ElasticTransport.DeserializeDocument<T>(source));
                }
            }
        }

        return results;
    }
}
