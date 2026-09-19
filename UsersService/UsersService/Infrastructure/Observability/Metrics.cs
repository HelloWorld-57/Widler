using System.Diagnostics.Metrics;

namespace UsersService.Infrastructure.Observability
{
    internal static class Metrics
    {
        public static readonly Meter Meter = new("UsersService", "1.0.0");

        public static readonly Counter<long> OutboxProcessed = Meter.CreateCounter<long>("outbox_processed_total");

        public static readonly Counter<long> DlqMessages = Meter.CreateCounter<long>("dlq_messages_total");

        public static readonly Histogram<double> OutboxProcessingDuration = 
            Meter.CreateHistogram<double>(
                "outbox_processing_duration", 
                unit: "ms");

        private static int _outboxSize;
        public static readonly ObservableGauge<int> OutboxSize = Meter.CreateObservableGauge("outbox_size", () => _outboxSize);

        public static void SetOutboxSize(int size) => _outboxSize = size;

        // Reconciliation

        public static readonly Counter<long> ReconciliationRuns =
            Meter.CreateCounter<long>(
                "reconciliation_runs_total",
                description: "Number of reconciliation runs.");

        public static readonly Counter<long> ReconciliationErrors =
            Meter.CreateCounter<long>(
                "reconciliation_errors_total",
                description: "Number of failed reconciliation runs.");

        public static readonly Counter<long> ReconciliationSkipped =
            Meter.CreateCounter<long>(
                "reconciliation_skipped_total",
                description: "Number of reconciliation runs skipped because the distributed lock was unavailable.");

        public static readonly Counter<long> ReconciliationUsersCreated =
            Meter.CreateCounter<long>(
                "reconciliation_users_created_total",
                description: "Number of users created during reconciliation.");

        public static readonly Counter<long> ReconciliationUsersUpdated =
            Meter.CreateCounter<long>(
                "reconciliation_users_updated_total",
                description: "Number of users updated during reconciliation.");

        public static readonly Counter<long> ReconciliationUsersRestored =
            Meter.CreateCounter<long>(
                "reconciliation_users_restored_total",
                description: "Number of users restored during reconciliation.");

        public static readonly Counter<long> ReconciliationUsersDeleted =
            Meter.CreateCounter<long>(
                "reconciliation_users_deleted_total",
                description: "Number of users deleted during reconciliation.");

        public static readonly Histogram<double> ReconciliationDuration =
            Meter.CreateHistogram<double>(
                "reconciliation_duration",
                unit: "ms",
                description: "Duration of reconciliation runs.");
    }
}
