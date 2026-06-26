using System.Security.Cryptography.X509Certificates;

namespace SquidStd.Network.Data.Options;

public sealed class SquidStdWebSocketServerTlsOptions
{
    public SquidStdWebSocketServerTlsOptions(X509Certificate2 serverCertificate)
    {
        ArgumentNullException.ThrowIfNull(serverCertificate);

        ServerCertificate = serverCertificate;
    }

    public X509Certificate2 ServerCertificate { get; }
}
