using System.Runtime.CompilerServices;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using SquidStd.Core.Interfaces.Secrets;
using SquidStd.Secrets.Aws.Data;
using SquidStd.Secrets.Aws.Internal;

namespace SquidStd.Secrets.Aws.Services;

/// <summary>An <see cref="ISecretStore" /> backed by AWS Secrets Manager.</summary>
public sealed class AwsSecretsManagerStore : ISecretStore, IDisposable
{
    private readonly AmazonSecretsManagerClient _client;
    private readonly string _prefix;

    public AwsSecretsManagerStore(AwsSecretsManagerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _prefix = options.NamePrefix ?? string.Empty;
        _client = new AmazonSecretsManagerClient(
            AwsClientFactory.Credentials(options.Aws), AwsClientFactory.SecretsManagerConfig(options.Aws)
        );
    }

    /// <inheritdoc />
    public async ValueTask<bool> DeleteAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!await ExistsAsync(name, cancellationToken).ConfigureAwait(false))
        {
            return false;
        }

        try
        {
            await _client.DeleteSecretAsync(
                new DeleteSecretRequest { SecretId = _prefix + name, ForceDeleteWithoutRecovery = true },
                cancellationToken
            ).ConfigureAwait(false);

            return true;
        }
        catch (ResourceNotFoundException)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async ValueTask<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        try
        {
            await _client.DescribeSecretAsync(
                new DescribeSecretRequest { SecretId = _prefix + name }, cancellationToken
            ).ConfigureAwait(false);

            return true;
        }
        catch (ResourceNotFoundException)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async ValueTask<string?> GetAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        try
        {
            var response = await _client.GetSecretValueAsync(
                new GetSecretValueRequest { SecretId = _prefix + name }, cancellationToken
            ).ConfigureAwait(false);

            return response.SecretString;
        }
        catch (ResourceNotFoundException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async ValueTask SetAsync(string name, string value, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(value);

        var secretId = _prefix + name;

        try
        {
            await _client.PutSecretValueAsync(
                new PutSecretValueRequest { SecretId = secretId, SecretString = value }, cancellationToken
            ).ConfigureAwait(false);
        }
        catch (ResourceNotFoundException)
        {
            await _client.CreateSecretAsync(
                new CreateSecretRequest { Name = secretId, SecretString = value }, cancellationToken
            ).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> ListNamesAsync(string? prefix = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var fullPrefix = _prefix + (prefix ?? string.Empty);
        string? token = null;

        do
        {
            var response = await _client.ListSecretsAsync(
                new ListSecretsRequest { NextToken = token, MaxResults = 100 }, cancellationToken
            ).ConfigureAwait(false);

            foreach (var secret in response.SecretList)
            {
                if (secret.Name.StartsWith(fullPrefix, StringComparison.Ordinal))
                {
                    yield return secret.Name[_prefix.Length..];
                }
            }

            token = response.NextToken;
        }
        while (!string.IsNullOrEmpty(token));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
    }
}
