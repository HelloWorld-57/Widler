using System.Text.Json.Serialization;

namespace UsersService.Infrastructure.Messaging.Kafka.Events.Keycloak
{
    public sealed record KeycloakAdminEvent
    {
        [JsonPropertyName("realmId")]
        public string? RealmId { get; init; }

        [JsonPropertyName("realmName")]
        public string? RealmName { get; init; }

        [JsonPropertyName("resourceType")]
        public string? ResourceType { get; init; }

        [JsonPropertyName("operationType")]
        public string? OperationType { get; init; }

        [JsonPropertyName("resourcePath")]
        public string? ResourcePath { get; init; }

        [JsonPropertyName("time")]
        public long Time { get; init; }

        [JsonPropertyName("authRealmId")]
        public string? AuthRealmId { get; init; }

        [JsonPropertyName("authClientId")]
        public string? AuthClientId { get; init; }

        [JsonPropertyName("authUserId")]
        public string? AuthUserId { get; init; }

        [JsonPropertyName("authIpAddress")]
        public string? AuthIpAddress { get; init; }

        [JsonPropertyName("representation")]
        public string? Representation { get; init; }
    }
}
