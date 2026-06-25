namespace SquidStd.Messaging.Sqs.Internal;

/// <summary>
/// Sanitizes queue/topic names to the SQS/SNS allowed alphabet (letters, digits, '-', '_').
/// </summary>
internal static class SqsNames
{
    public static string Sanitize(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var buffer = new char[name.Length];

        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            buffer[i] = char.IsAsciiLetterOrDigit(c) || c is '-' or '_' ? c : '-';
        }

        return new(buffer);
    }
}
