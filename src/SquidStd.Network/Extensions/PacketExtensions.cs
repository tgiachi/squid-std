namespace SquidStd.Network.Extensions;

public static class PacketExtensions
{
    extension(byte opCode)
    {
        public string ToPacketString()
            => "0x" + opCode.ToString("X2");
    }
}
