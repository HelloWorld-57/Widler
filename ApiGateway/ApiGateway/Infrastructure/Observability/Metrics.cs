using System.Diagnostics.Metrics;

namespace ApiGateway.Infrastructure.Observability
{
    internal static class Metrics
    {
        public static readonly Meter Meter = new("ApiGateway", "1.0.0");

        
    }
}
