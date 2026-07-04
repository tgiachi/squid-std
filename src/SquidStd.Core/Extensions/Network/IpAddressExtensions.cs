using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;

namespace SquidStd.Core.Extensions.Network;

/// <summary>
/// Conversions between IP addresses and their raw 32-bit representation.
/// </summary>
public static class IpAddressExtensions
{
    /// <summary>
    /// Returns the IPv4 address of the endpoint as a little-endian 32-bit value.
    /// </summary>
    /// <exception cref="InvalidOperationException">The address has no IPv4 representation.</exception>
    public static uint ToRawAddress(this IPEndPoint endPoint)
        => endPoint.Address.ToRawAddress();

    /// <summary>
    /// Returns the IPv4 address as a little-endian 32-bit value. IPv6-mapped IPv4 addresses are unwrapped.
    /// </summary>
    /// <exception cref="InvalidOperationException">The address has no IPv4 representation.</exception>
    public static uint ToRawAddress(this IPAddress ipAddress)
    {
        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6 && !ipAddress.IsIPv4MappedToIPv6)
        {
            throw new InvalidOperationException("IP address could not be serialized to a 32-bit value.");
        }

        Span<byte> bytes = stackalloc byte[4];

        if (!ipAddress.MapToIPv4().TryWriteBytes(bytes, out var bytesWritten) || bytesWritten != 4)
        {
            throw new InvalidOperationException("IP address could not be serialized to a 32-bit value.");
        }

        return BinaryPrimitives.ReadUInt32LittleEndian(bytes);
    }
}
