using Confluent.Kafka;

namespace UsersService.Infrastructure.Messaging.Kafka.Events.Keycloak
{
    public interface IKeycloakAdminEventProcessor
    {
        Task ProcessAsync(ConsumeResult<string, string> result, CancellationToken ct);
    }
}
