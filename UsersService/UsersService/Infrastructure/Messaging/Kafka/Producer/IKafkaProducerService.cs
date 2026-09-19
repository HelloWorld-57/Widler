using UsersService.Infrastructure.Events.Kafka.Messages.Users.v1;
using UsersService.Infrastructure.Messaging.Kafka.Events.Users.v1;

namespace UsersService.Infrastructure.Messaging.Kafka.Producer
{
    public interface IKafkaProducerService
    {
        Task PublishUserCreatedAsync(
            UserCreatedV1 evt, 
            string? traceParent,
            string? traceState,
            string? correlationId,
            CancellationToken ct);
        Task PublishUserUpdatedAsync(
            UserUpdatedV1 evt,
            string? traceParent,
            string? traceState,
            string? correlationId,
            CancellationToken ct);
        Task PublishUserDeletedAsync(
            UserDeletedV1 evt,
            string? traceParent,
            string? traceState,
            string? correlationId,
            CancellationToken ct);
    }
}
