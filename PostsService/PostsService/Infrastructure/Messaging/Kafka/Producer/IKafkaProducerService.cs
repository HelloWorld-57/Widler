using PostsService.Infrastructure.Events.Kafka.Messages.Posts.v1;

namespace PostsService.Infrastructure.Messaging.Kafka.Producer
{
    public interface IKafkaProducerService
    {
        Task PublishPostCreatedAsync(PostCreatedV1 evt, CancellationToken ct);
        Task PublishPostUpdatedAsync(PostUpdatedV1 evt, CancellationToken ct);
        Task PublishPostDeletedAsync(PostDeletedV1 evt, CancellationToken ct);
    }
}
