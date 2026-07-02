<h1 align="center">SquidStd.Aws.Abstractions</h1>

Shared, dependency-free AWS connection config for SquidStd providers built on the AWS SDK
(e.g. `SquidStd.Messaging.Sqs`). A single `AwsConfigEntry` carries region, credentials and an
optional endpoint override (for LocalStack or other S3/SQS-compatible endpoints).

## Install

```bash
dotnet add package SquidStd.Aws.Abstractions
```

## Usage

```csharp
using SquidStd.Aws.Abstractions.Data.Config;

var aws = new AwsConfigEntry
{
    Region = "eu-west-1",
    // AccessKey/SecretKey omitted -> the AWS default credential chain is used
    ServiceUrl = null,        // set to http://localhost:4566 for LocalStack
};
```

When `AccessKey`/`SecretKey` are null, consumers fall back to the AWS default credential chain
(environment variables, shared profile, IAM role). When `ServiceUrl` is set, consumers point their
client at that endpoint instead of the regional AWS endpoint.

## Key types

| Type             | Purpose                                                                           |
|------------------|-----------------------------------------------------------------------------------|
| `AwsConfigEntry` | Region, optional credentials and an optional endpoint override (e.g. LocalStack). |

## Related

- Article: [AWS abstractions](https://tgiachi.github.io/squid-std/articles/aws-abstractions.html)
- [`SquidStd.Secrets.Aws`](https://tgiachi.github.io/squid-std/articles/secrets-aws.html) - AWS Secrets Manager provider using this config
- [`SquidStd.Messaging.Sqs`](https://tgiachi.github.io/squid-std/articles/messaging-sqs.html) - SQS messaging provider using this config
- [`SquidStd.Storage.S3`](https://tgiachi.github.io/squid-std/articles/storage-s3.html) - S3 storage provider using this config

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
