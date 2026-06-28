namespace SquidStd.Vfs.Abstractions.Interfaces;

/// <summary>A virtual filesystem that is locked until unlocked with a passphrase (e.g. an encrypted vault).</summary>
public interface ILockableFileSystem : IVirtualFileSystem
{
    /// <summary>Whether the filesystem is currently unlocked.</summary>
    bool IsUnlocked { get; }

    /// <summary>Derives the key from the passphrase and unlocks the filesystem.</summary>
    void Unlock(string passphrase);

    /// <summary>Zeroes and drops the key, locking the filesystem.</summary>
    void Lock();
}
