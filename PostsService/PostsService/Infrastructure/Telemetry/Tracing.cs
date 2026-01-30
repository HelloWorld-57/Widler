using System.Diagnostics;

namespace PostsService.Infrastructure.Telemetry
{
    public static class Tracing
    {
        public const string SourceName = "PostsService";

        public static readonly ActivitySource ActivitySource = new("PostsService");
    }
}
