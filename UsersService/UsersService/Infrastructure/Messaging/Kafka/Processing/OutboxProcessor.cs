using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UsersService.Infrastructure.Db;
using UsersService.Infrastructure.Db.Outbox;
using UsersService.Infrastructure.Events.Kafka.Messages.Users.v1;
using UsersService.Infrastructure.Messaging.Kafka.Events;
using UsersService.Infrastructure.Messaging.Kafka.Events.Users.v1;
using UsersService.Infrastructure.Messaging.Kafka.Producer;
using UsersService.Infrastructure.Messaging.Kafka.Serialization;
using UsersService.Infrastructure.Observability;

namespace UsersService.Infrastructure.Messaging.Kafka.Processing
{
    public sealed class OutboxProcessor : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<OutboxProcessor> _logger;
        private readonly IKafkaProducerService _producer;

        public OutboxProcessor(
            IServiceProvider services,
            ILogger<OutboxProcessor> logger,
            IKafkaProducerService producer)
        {
            _services = services;
            _logger = logger;
            _producer = producer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

            var pendingCount = await db.OutboxMessages.CountAsync(x => x.ProcessedAt == null);
            Metrics.SetOutboxSize(pendingCount);

            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessBatchAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        private async Task ProcessBatchAsync(CancellationToken ct)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

            var messages = await db.OutboxMessages
                .Where(x => x.ProcessedAt == null && x.Attempts < 10)
                .OrderBy(x => x.OccurredAt)
                .Take(20)
                .ToListAsync(ct);

            foreach (var msg in messages)
            {
                try
                {
                    await PublishOutboxMessageAsync(msg, ct);
                    msg.ProcessedAt = DateTime.UtcNow;

                    Metrics.OutboxProcessed.Add(1);
                }
                catch (Exception ex)
                {
                    msg.Attempts++;

                    _logger.LogError(
                        ex,
                        "Failed to publish outbox message {MessageId}",
                        msg.Id);
                }
            }

            await db.SaveChangesAsync(ct);

            var pendingCount = await db.OutboxMessages.CountAsync(x => x.ProcessedAt == null);
            Metrics.SetOutboxSize(pendingCount);
        }

        private async Task PublishOutboxMessageAsync(OutboxMessage msg, CancellationToken ct)
        {
            switch (msg.EventType)
            {
                case IntegrationEventNames.Users.CreatedV1:
                    {
                        var evt = JsonSerializer.Deserialize<UserCreatedV1>(msg.Payload, JsonOptions.Default)!;
                        await _producer.PublishUserCreatedAsync(evt, ct);
                        break;
                    }
                case IntegrationEventNames.Users.UpdatedV1:
                    {
                        var evt = JsonSerializer.Deserialize<UserUpdatedV1>(msg.Payload, JsonOptions.Default)!;
                        await _producer.PublishUserUpdatedAsync(evt, ct);
                        break;
                    }
                case IntegrationEventNames.Users.DeletedV1:
                    {
                        var evt = JsonSerializer.Deserialize<UserDeletedV1>(msg.Payload, JsonOptions.Default)!;
                        await _producer.PublishUserDeletedAsync(evt, ct);
                        break;
                    }
                default:
                    throw new InvalidOperationException(
                        $"No Kafka publisher configured for outbox message type '{msg.EventType}'");
            }
        }
    }
}
