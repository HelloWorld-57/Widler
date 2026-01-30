using Microsoft.EntityFrameworkCore;
using PostsService.Infrastructure.Db;
using PostsService.Infrastructure.Db.Outbox;
using PostsService.Infrastructure.Events.Kafka.Messages.Posts.v1;
using PostsService.Infrastructure.Messaging.Kafka.Events;
using PostsService.Infrastructure.Messaging.Kafka.Producer;
using PostsService.Infrastructure.Messaging.Kafka.Serialization;
using PostsService.Infrastructure.Observability;
using System.Text.Json;

namespace PostsService.Infrastructure.Messaging.Kafka.Processing
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
            var db = scope.ServiceProvider.GetRequiredService<PostsDbContext>();

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
            var db = scope.ServiceProvider.GetRequiredService<PostsDbContext>();

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
                case IntegrationEventNames.Posts.CreatedV1:
                    {
                        var evt = JsonSerializer.Deserialize<PostCreatedV1>(msg.Payload, JsonOptions.Default)!;
                        await _producer.PublishPostCreatedAsync(evt, ct);
                        break;
                    }
                case IntegrationEventNames.Posts.UpdatedV1:
                    {
                        var evt = JsonSerializer.Deserialize<PostUpdatedV1>(msg.Payload, JsonOptions.Default)!;
                        await _producer.PublishPostUpdatedAsync(evt, ct);
                        break;
                    }
                case IntegrationEventNames.Posts.DeletedV1:
                    {
                        var evt = JsonSerializer.Deserialize<PostDeletedV1>(msg.Payload, JsonOptions.Default)!;
                        await _producer.PublishPostDeletedAsync(evt, ct);
                        break;
                    }
                default:
                    throw new InvalidOperationException(
                        $"No Kafka publisher configured for outbox message type '{msg.EventType}'");
            }
        }
    }
}
