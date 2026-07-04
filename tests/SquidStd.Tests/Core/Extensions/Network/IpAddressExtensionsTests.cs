using System.Net;
using SquidStd.Core.Extensions.Network;

namespace SquidStd.Tests.Core.Extensions.Network;

public class IpAddressExtensionsTests
{
    [Fact]
    public void ToRawAddress_LoopbackIsLittleEndianOfBytes()
    {
        // 127.0.0.1 -> bytes { 127, 0, 0, 1 } read little-endian = 0x0100007F
        Assert.Equal(0x0100007Fu, IPAddress.Loopback.ToRawAddress());
    }

    [Fact]
    public void ToRawAddress_Ipv6MappedIpv4_IsUnwrapped()
        => Assert.Equal(0x0100007Fu, IPAddress.Parse("::ffff:127.0.0.1").ToRawAddress());

    [Fact]
    public void ToRawAddress_EndpointDelegatesToAddress()
    {
        var endPoint = new IPEndPoint(IPAddress.Parse("192.168.1.10"), 2593);

        Assert.Equal(IPAddress.Parse("192.168.1.10").ToRawAddress(), endPoint.ToRawAddress());
    }

    [Fact]
    public void ToRawAddress_PureIpv6_Throws()
        => Assert.Throws<InvalidOperationException>(() => IPAddress.IPv6Loopback.ToRawAddress());
}
