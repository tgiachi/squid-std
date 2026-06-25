using OpenTelemetry.Exporter;
using SquidStd.Telemetry.Abstractions.Types.Telemetry;
using SquidStd.Telemetry.OpenTelemetry.Internal;

namespace SquidStd.Tests.Telemetry;

public class OtlpProtocolTypeMappingTests
{
    [Fact]
    public void Grpc_MapsToGrpc()
        => Assert.Equal(OtlpExportProtocol.Grpc, TelemetryPipeline.Map(OtlpProtocolType.Grpc));

    [Fact]
    public void HttpProtobuf_MapsToHttpProtobuf()
        => Assert.Equal(OtlpExportProtocol.HttpProtobuf, TelemetryPipeline.Map(OtlpProtocolType.HttpProtobuf));
}
