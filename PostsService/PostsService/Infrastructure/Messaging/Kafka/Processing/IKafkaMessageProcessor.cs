namespace PostsService.Infrastructure.Messaging.Kafka.Processing
{
    public interface IKafkaMessageProcessor
    {
        Task ProcessAsync(string eventType, string payload, CancellationToken ct);
    }
}
