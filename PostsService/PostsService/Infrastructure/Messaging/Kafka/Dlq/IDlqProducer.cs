using PostsService.Infrastructure.Messaging.Kafka.Dlq;

namespace PostsService.Infrastructure.Messaging.Kafka.DlqProducer
{
    public interface IDlqProducer
    {
        Task PublishAsync(DlqMessage message, CancellationToken ct);
    }
}
