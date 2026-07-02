# Security

SquidStd ships focused primitives for the three common secret-handling needs:
hashing credentials, protecting payloads with a key, and storing named secrets.

## Steps

1. **Hash passwords** with `HashUtils` (PBKDF2-SHA256). Never store plaintext.

   ```csharp
   var stored = HashUtils.HashPassword(password);
   var ok = HashUtils.VerifyPassword(password, stored);
   ```

2. **Protect payloads** through `ISecretProtector`. The default
   (`SquidStd.Services.Core`) is AES-GCM; for managed keys use
   `RegisterKmsSecretProtector` from `SquidStd.Secrets.Aws`.

   ```csharp
   byte[] protectedData = protector.Protect(plaintext);
   byte[] clear = protector.Unprotect(protectedData);
   ```

3. **Store named secrets** behind `ISecretStore`. The default is file-backed; use
   `RegisterAwsSecretsManagerStore` (`SquidStd.Secrets.Aws`) for AWS Secrets Manager.

   ```csharp
   await store.SetAsync("db/password", value);
   var secret = await store.GetAsync("db/password");
   ```

4. **Use a PGP keyring** for armored encryption/signing by registering a key
   store with `RegisterPgp` from `SquidStd.Crypto`.

   ```csharp
   container.RegisterPgp(resolver => myPgpKeyStore);
   ```

## Recommendation

Use `HashUtils` for credentials, `ISecretProtector` (AES-GCM locally, KMS in the
cloud) for encrypting blobs, and `ISecretStore` for named secrets - backed by
files in development and AWS Secrets Manager in production.
