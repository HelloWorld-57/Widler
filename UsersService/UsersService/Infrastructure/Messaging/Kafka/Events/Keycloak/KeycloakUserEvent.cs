using System.Text.Json.Serialization;

namespace UsersService.Infrastructure.Messaging.Kafka.Events.Keycloak
{
    public sealed record KeycloakUserEvent
    {
        [JsonPropertyName("realmId")]
        public string? RealmId { get; init; }

        [JsonPropertyName("realmName")]
        public string? RealmName { get; init; }

        [JsonPropertyName("clientId")]
        public string? ClientId { get; init; }

        [JsonPropertyName("userId")]
        public string? UserId { get; init; }

        [JsonPropertyName("ipAddress")]
        public string? IpAddress { get; init; }

        [JsonPropertyName("eventType")]
        public string? EventType { get; init; }

        [JsonPropertyName("time")]
        public long Time { get; init; }

        [JsonPropertyName("details")]
        public Dictionary<string, string>? Details { get; init; }
    }
}
