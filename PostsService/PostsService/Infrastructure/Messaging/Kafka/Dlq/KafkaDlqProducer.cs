using Confluent.Kafka;
using Microsoft.Extensions.Options;
using PostsService.Infrastructure.Messaging.Kafka.DlqProducer;
using PostsService.Infrastructure.Messaging.Kafka.Events;
using PostsService.Infrastructure.Messaging.Kafka.Serialization;
using PostsService.Infrastructure.Observability;
using PostsService.Infrastructure.Telemetry;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace PostsService.Infrastructure.Messaging.Kafka.Dlq
{
    public sealed class KafkaDlqProducer : IDlqProducer, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaDlqProducer> _logger;
        private readonly KafkaTopics _topics;

        public KafkaDlqProducer(
            IConfiguration config,
            IOptions<KafkaTopics> topics,
            ILogger<KafkaDlqProducer> logger)
        {
            _logger = logger;
            _topics = topics.Value;

            var conf = new ProducerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"],
                Acks = Acks.All,            // Wait for all replicas to acknowledge
                EnableIdempotence = true,   // Ensure exactly-once semantics
                MessageSendMaxRetries = 3,  // Retry 3 times
                RetryBackoffMs = 100,        // Wait 100ms between retries
                MessageTimeoutMs = int.Parse(config["Kafka:SocketTimeoutMs"] ?? "60000"),   // Максимальное время, которое producer будет пытаться доставить сообщение
                SocketTimeoutMs = int.Parse(config["Kafka:SocketTimeoutMs"] ?? "60000"),    // Максимальное время, которое клиент будет ждать ответа от брокера Kafka по сети
                ReconnectBackoffMs = int.Parse(config["Kafka:ReconnectBackoffMs"] ?? "5000"),   // Максимальное время, которое producer будет пытаться доставить сообщение
                ReconnectBackoffMaxMs = int.Parse(config["Kafka:ReconnectBackoffMaxMs"] ?? "30000") // Максимальный интервал между попытками переподключения.

            };

            _producer = new ProducerBuilder<string, string>(conf).Build();
        }

        public async Task PublishAsync(DlqMessage msg, CancellationToken ct)
        {
            using var activity = Tracing.ActivitySource.StartActivity("Kafka.Publish", ActivityKind.Producer);
            activity?.SetTag("messaging.system", "kafka");
            activity?.SetTag("messaging.destination", _topics.PostsDlq);

            var json = JsonSerializer.Serialize(msg, JsonOptions.Default);

            var kafkaMsg = new Message<string, string>
            {
                Key = msg.Key ?? Guid.NewGuid().ToString(),
                Value = json,
                Headers = new Headers
                {
                    { "event-type", Encoding.UTF8.GetBytes(IntegrationEventNames.Dlq.MessageV1) },
                    { "original-topic", Encoding.UTF8.GetBytes(msg.OriginalTopic) },
                    { "original-partition", Encoding.UTF8.GetBytes(msg.Partition.ToString()) },
                    { "original-offset", Encoding.UTF8.GetBytes(msg.Offset.ToString()) }
                }
            };

            if (!string.IsNullOrWhiteSpace(msg.TraceId))
            {
                kafkaMsg.Headers.Add("traceparent", Encoding.UTF8.GetBytes(msg.TraceId));
            }

            try
            {
                await _producer.ProduceAsync(
                    _topics.PostsDlq,
                    kafkaMsg,
                    ct);

                Metrics.DlqMessages.Add(1);

                _logger.LogError(
                    "Message sent to DLQ. Topic={Topic}, Offset={Offset}",
                    msg.OriginalTopic,
                    msg.Offset);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(
                    ex,
                    "FAILED to send message to DLQ. Message LOST. Topic={Topic}, Offset={Offset}",
                    msg.OriginalTopic,
                    msg.Offset);
            }
        }

        public void Dispose() => _producer.Dispose();
    }
}
