using System.Diagnostics;

namespace ApiGateway.Infrastructure.Telemetry
{
    public static class Tracing
    {
        public const string SourceName = "ApiGateway";

        public static readonly ActivitySource ActivitySource = new("ApiGateway");
    }
}
