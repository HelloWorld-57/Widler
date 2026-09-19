using Confluent.Kafka;

namespace UsersService.Infrastructure.Messaging.Kafka.Events.Keycloak
{
    public interface IKeycloakEventProcessor
    {
        Task ProcessAsync(ConsumeResult<string, string> result, CancellationToken ct);
    }
}
