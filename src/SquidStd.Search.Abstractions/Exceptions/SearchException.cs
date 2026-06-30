namespace SquidStd.Search.Abstractions.Exceptions;

/// <summary>Thrown when Elasticsearch returns a non-successful response.</summary>
public sealed class SearchException : Exception
{
    /// <summary>Initializes the exception with a message.</summary>
    public SearchException(string message)
        : base(message) { }

    /// <summary>Initializes the exception with a message and inner exception.</summary>
    public SearchException(string message, Exception innerException)
        : base(message, innerException) { }
}
