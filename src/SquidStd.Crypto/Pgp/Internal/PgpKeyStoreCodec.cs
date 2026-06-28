using System.Text;
using SquidStd.Crypto.Pgp.Data;

namespace SquidStd.Crypto.Pgp.Internal;

/// <summary>
///     Encodes a set of keys into a single byte blob (one base64 record per line) and back. Only armored
///     material is stored; metadata is re-derived on load.
/// </summary>
internal static class PgpKeyStoreCodec
{
    public static byte[] Encode(IReadOnlyCollection<PgpKey> keys)
    {
        var builder = new StringBuilder();

        foreach (var key in keys)
        {
            var pub = Convert.ToBase64String(Encoding.UTF8.GetBytes(key.PublicArmored));
            var sec = key.PrivateArmored is null
                ? string.Empty
                : Convert.ToBase64String(Encoding.UTF8.GetBytes(key.PrivateArmored));
            builder.Append(pub).Append('\t').Append(sec).Append('\n');
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    public static IReadOnlyList<PgpKey> Decode(byte[] blob)
    {
        var text = Encoding.UTF8.GetString(blob);
        var result = new List<PgpKey>();

        foreach (var line in text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split('\t');
            var publicArmored = Encoding.UTF8.GetString(Convert.FromBase64String(parts[0]));
            string? privateArmored = parts.Length > 1 && parts[1].Length > 0
                ? Encoding.UTF8.GetString(Convert.FromBase64String(parts[1]))
                : null;

            result.Add(PgpKeyFactory.FromArmored(publicArmored, privateArmored));
        }

        return result;
    }
}
