using System.Diagnostics.Metrics;

namespace UsersService.Infrastructure.Observability
{
    internal static class Metrics
    {
        public static readonly Meter Meter = new("UsersService", "1.0.0");

        public static readonly Counter<long> OutboxProcessed = Meter.CreateCounter<long>("outbox_processed_total");

        public static readonly Counter<long> DlqMessages = Meter.CreateCounter<long>("dlq_messages_total");

        public static readonly Histogram<double> OutboxProcessingDuration = Meter.CreateHistogram<double>("outbox_processing_duration_ms", unit: "ms");

        private static int _outboxSize;
        public static readonly ObservableGauge<int> OutboxSize = Meter.CreateObservableGauge("outbox_size", () => _outboxSize);

        public static void SetOutboxSize(int size) => _outboxSize = size;
    }
}
