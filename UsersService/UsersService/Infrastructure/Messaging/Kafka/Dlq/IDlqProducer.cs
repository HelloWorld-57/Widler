using UsersService.Infrastructure.Messaging.Kafka.Dlq;

namespace UsersService.Infrastructure.Messaging.Kafka.DlqProducer
{
    public interface IDlqProducer
    {
        Task PublishAsync(DlqMessage message, CancellationToken ct);
    }
}
