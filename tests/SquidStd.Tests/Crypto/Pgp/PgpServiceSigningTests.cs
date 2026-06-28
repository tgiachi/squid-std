using System.Text;
using SquidStd.Crypto.Pgp.Services;
using SquidStd.Tests.Crypto.Pgp.Support;

namespace SquidStd.Tests.Crypto.Pgp;

[Collection("PgpKeys")]
public class PgpServiceSigningTests
{
    private readonly PgpTestKeys _keys;

    public PgpServiceSigningTests(PgpTestKeys keys)
    {
        _keys = keys;
    }

    [Fact]
    public async Task Sign_ThenVerify_ValidAndRecoversData()
    {
        var keyring = new PgpKeyring();
        keyring.Import(_keys.AlicePrivate);
        var service = new PgpService(keyring);
        var payload = Encoding.UTF8.GetBytes("signed by alice");

        var signed = await service.SignAsync(payload, PgpTestKeys.AliceIdentity, PgpTestKeys.AlicePassphrase);
        var result = await service.VerifyAsync(signed);

        Assert.True(result.IsValid);
        Assert.Equal(payload, result.Data);
    }

    [Fact]
    public async Task Verify_TamperedMessage_IsInvalid()
    {
        var keyring = new PgpKeyring();
        keyring.Import(_keys.AlicePrivate);
        var service = new PgpService(keyring);

        var signed = await service.SignAsync(
            Encoding.UTF8.GetBytes("original"),
            PgpTestKeys.AliceIdentity,
            PgpTestKeys.AlicePassphrase
        );
        var tampered = MutateBody(signed);

        var result = await service.VerifyAsync(tampered);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task EncryptAndSign_ThenDecryptAndVerify_RoundTripsAndValidates()
    {
        var keyring = new PgpKeyring();
        keyring.Import(_keys.AlicePrivate); // recipient (has secret to decrypt)
        keyring.Import(_keys.BobPrivate);   // signer
        var service = new PgpService(keyring);
        var payload = Encoding.UTF8.GetBytes("confidential and signed");

        var armored = await service.EncryptAndSignForAsync(
            PgpTestKeys.AliceIdentity,
            payload,
            PgpTestKeys.BobIdentity,
            PgpTestKeys.BobPassphrase
        );
        var result = await service.DecryptAndVerifyAsync(armored, PgpTestKeys.AlicePassphrase);

        Assert.Equal(payload, result.Data);
        Assert.True(result.IsSigned);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task DecryptAndVerify_SignerNotInKeyring_RecoversDataButInvalid()
    {
        // Sign with Bob, then decrypt on a keyring that lacks Bob's public key.
        var signingKeyring = new PgpKeyring();
        signingKeyring.Import(_keys.AlicePrivate);
        signingKeyring.Import(_keys.BobPrivate);
        var signingService = new PgpService(signingKeyring);
        var payload = Encoding.UTF8.GetBytes("signed by an unknown party");
        var armored = await signingService.EncryptAndSignForAsync(
            PgpTestKeys.AliceIdentity,
            payload,
            PgpTestKeys.BobIdentity,
            PgpTestKeys.BobPassphrase
        );

        var recipientOnly = new PgpKeyring();
        recipientOnly.Import(_keys.AlicePrivate); // no Bob public key
        var service = new PgpService(recipientOnly);

        var result = await service.DecryptAndVerifyAsync(armored, PgpTestKeys.AlicePassphrase);

        Assert.Equal(payload, result.Data);
        Assert.True(result.IsSigned);
        Assert.False(result.IsValid);
    }

    private static string MutateBody(string signed)
    {
        // Flip a character in the middle of the armored body to corrupt the signed payload.
        var lines = signed.Split('\n');
        var mid = lines.Length / 2;

        if (lines[mid].Length > 4)
        {
            var chars = lines[mid].ToCharArray();
            chars[2] = chars[2] == 'A' ? 'B' : 'A';
            lines[mid] = new string(chars);
        }

        return string.Join('\n', lines);
    }
}
