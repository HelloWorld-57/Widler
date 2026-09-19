using UsersService.Application.Messaging;

namespace UsersService.Infrastructure.Messaging.Kafka.Processing
{
    public sealed class KafkaMessageProcessor : IKafkaMessageProcessor
    {
        private readonly IIntegrationEventRouter _router;
        private readonly ILogger<KafkaMessageProcessor> _logger;
        private readonly int _maxAttempts = 5;
        private static readonly TimeSpan _baseDelay = TimeSpan.FromSeconds(2);

        public KafkaMessageProcessor(
            IIntegrationEventRouter router,
            ILogger<KafkaMessageProcessor> logger)
        {
            _router = router;
            _logger = logger;
        }

        public async Task ProcessAsync(string eventType, string payload, CancellationToken ct)
        {
            int attempt = 0;
            Exception? last = null;

            while (attempt < _maxAttempts)
            {
                attempt++;
                try
                {
                    await _router.RouteAsync(eventType, payload, ct);
                    return;
                }
                catch (Exception ex) when (IsRetryable(ex))
                {
                    last = ex;

                    _logger.LogWarning(ex, "Integration handler failed. Attempt {Attempt}", attempt);

                    await Task.Delay(CalculateBackoff(attempt), ct);
                }
            }

            throw last!;
        }

        private static bool IsRetryable(Exception ex)
            => ex is not ArgumentException; // пример

        private static TimeSpan CalculateBackoff(int attempt)
        {
            var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 300));
            return TimeSpan.FromSeconds(_baseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1)) + jitter;
        }

    }
}
