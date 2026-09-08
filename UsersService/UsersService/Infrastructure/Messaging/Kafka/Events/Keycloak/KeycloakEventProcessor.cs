using Confluent.Kafka;
using System.Text.Json;
using UsersService.Application.Commands;
using UsersService.Application.Interfaces;
using UsersService.Infrastructure.Messaging.Kafka.Serialization;

namespace UsersService.Infrastructure.Messaging.Kafka.Events.Keycloak
{
    public sealed class KeycloakEventProcessor : IKeycloakEventProcessor
    {
        private readonly IUserService _userService;
        private readonly ILogger<KeycloakEventProcessor> _logger;

        public KeycloakEventProcessor(ILogger<KeycloakEventProcessor> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public async Task ProcessAsync(ConsumeResult<string, string> result, CancellationToken ct)
        {
            var keycloakEvent = JsonSerializer.Deserialize<KeycloakUserEvent>(result.Message.Value, JsonOptions.Default);

            if (keycloakEvent is null)
            {
                throw new InvalidOperationException("Failed to deserialize Keycloak event.");
            }

            if (string.IsNullOrWhiteSpace(keycloakEvent.UserId))
            {
                throw new InvalidOperationException($"Keycloak event {keycloakEvent.EventType} does not contain userId.");
            }

            switch (keycloakEvent.EventType)
            {
                case "REGISTER":
                    await ProcessRegisterAsync(keycloakEvent, ct);
                    break;

                case "UPDATE_EMAIL":
                    await ProcessUpdateEmailAsync(keycloakEvent, ct);
                    break;

                default:
                    _logger.LogDebug("Ignoring Keycloak event. EventType={EventType}", keycloakEvent.EventType);
                    break;
            }
        }

        private async Task ProcessRegisterAsync(KeycloakUserEvent keycloakEvent, CancellationToken ct)
        {
            var details = keycloakEvent.Details;

            var username = details?.GetValueOrDefault("username");
            var email = details?.GetValueOrDefault("email");

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new InvalidOperationException("Keycloak REGISTER event does not contain username.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new InvalidOperationException("Keycloak REGISTER event does not contain email.");
            }

            await _userService.CreateFromKeycloakAsync(
                new CreateUserFromKeycloakCommand(
                    keycloakEvent.UserId!,
                    username,
                    email),
                ct);
        }

        private async Task ProcessUpdateEmailAsync(KeycloakUserEvent keycloakEvent, CancellationToken ct)
        {
            var updatedEmail = keycloakEvent.Details?.GetValueOrDefault("updated_email");

            if (string.IsNullOrWhiteSpace(updatedEmail))
            {
                throw new InvalidOperationException("Keycloak UPDATE_EMAIL event does not contain updated_email.");
            }

            await _userService.UpdateEmailFromKeycloakAsync(
                new UpdateUserEmailFromKeycloakCommand(
                    keycloakEvent.UserId!,
                    updatedEmail),
                ct);
        }
    }
}
