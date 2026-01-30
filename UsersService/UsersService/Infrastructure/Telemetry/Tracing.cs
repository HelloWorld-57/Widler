using System.Diagnostics;

namespace UsersService.Infrastructure.Telemetry
{
    public static class Tracing
    {
        public const string SourceName = "UsersService";

        public static readonly ActivitySource ActivitySource = new("UsersService");
    }
}
