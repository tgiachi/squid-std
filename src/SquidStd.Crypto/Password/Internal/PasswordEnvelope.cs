namespace SquidStd.Crypto.Password.Internal;

/// <summary>Parsed fields of a password-encryption envelope.</summary>
internal sealed class PasswordEnvelope
{
    public byte Version { get; }
    public int Iterations { get; }
    public int MemoryKib { get; }
    public int Parallelism { get; }
    public byte[] Salt { get; }
    public byte[] Nonce { get; }
    public byte[] Tag { get; }
    public byte[] Ciphertext { get; }

    public PasswordEnvelope(
        byte version,
        int iterations,
        int memoryKib,
        int parallelism,
        byte[] salt,
        byte[] nonce,
        byte[] tag,
        byte[] ciphertext
    )
    {
        Version = version;
        Iterations = iterations;
        MemoryKib = memoryKib;
        Parallelism = parallelism;
        Salt = salt;
        Nonce = nonce;
        Tag = tag;
        Ciphertext = ciphertext;
    }
}
