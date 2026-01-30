using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using UsersService.Infrastructure.Events.Kafka.Messages.Users.v1;
using UsersService.Infrastructure.Exceptions;
using UsersService.Infrastructure.Messaging.Kafka.Events;
using UsersService.Infrastructure.Messaging.Kafka.Events.Users.v1;
using UsersService.Infrastructure.Messaging.Kafka.Serialization;
using UsersService.Infrastructure.Telemetry;

namespace UsersService.Infrastructure.Messaging.Kafka.Producer
{
    public class KafkaProducerService : IKafkaProducerService, IDisposable
    {
        private readonly ILogger<KafkaProducerService> _logger;
        private readonly IProducer<string, string> _producer;
        private readonly KafkaTopics _topics;

        public KafkaProducerService(IConfiguration config, ILogger<KafkaProducerService> logger, IOptions<KafkaTopics> topics)
        {
            var kafkaConf = new ProducerConfig
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

            _topics = topics.Value;
            _logger = logger;
            _producer = new ProducerBuilder<string, string>(kafkaConf).Build();
        }

        private async Task PublishAsync<T>(
            string topic,
            string key,
            T payload,
            IDictionary<string, string> headers,
            CancellationToken ct)
        {
            using var activity = Tracing.ActivitySource.StartActivity("Kafka.Publish", ActivityKind.Producer);
            activity?.SetTag("messaging.system", "kafka");
            activity?.SetTag("messaging.destination", topic);

            var json = JsonSerializer.Serialize(payload, JsonOptions.Default);

            var msg = new Message<string, string>
            {
                Key = key,
                Value = json,
                Headers = new Headers()
            };

            foreach (var kv in headers) 
            {
                msg.Headers.Add(kv.Key, Encoding.UTF8.GetBytes(kv.Value));
            }

            if (!string.IsNullOrEmpty(Activity.Current?.Id))
            {
                msg.Headers.Add("traceparent", Encoding.UTF8.GetBytes(Activity.Current.Id));
            }

            try
            {
                var res = await _producer.ProduceAsync(topic, msg);
                _logger.LogInformation("Kafka: produced message to {TopicName}: {Message}", topic, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Kafka: failed to produce message to {TopicName}. Key={Key}. Value={Value}.",
                    topic,
                    key,
                    json
                );

                throw new KafkaPublishException(topic, key, json, ex);
            }
        }

        public Task PublishUserCreatedAsync(UserCreatedV1 evt, CancellationToken ct)
        => PublishAsync(
            topic: _topics.Users,
            key: evt.UserId,
            payload: evt,
            headers: new Dictionary<string, string>
            {
                ["event-type"] = IntegrationEventNames.Users.CreatedV1
            },
            ct);

        public Task PublishUserUpdatedAsync(UserUpdatedV1 evt, CancellationToken ct)
        => PublishAsync(
            topic: _topics.Users,
            key: evt.UserId,
            payload: evt,
            headers: new Dictionary<string, string>
            {
                ["event-type"] = IntegrationEventNames.Users.UpdatedV1
            },
            ct);

        public Task PublishUserDeletedAsync(UserDeletedV1 evt, CancellationToken ct)
        => PublishAsync(
            topic: _topics.Users,
            key: evt.UserId,
            payload: evt,
            headers: new Dictionary<string, string>
            {
                ["event-type"] = IntegrationEventNames.Users.DeletedV1
            },
            ct);

        public void Dispose() => _producer?.Dispose();
    }
}
