using Confluent.Kafka;
using System.Text.Json;
using UsersService.Application.Commands;
using UsersService.Application.Interfaces;
using UsersService.Infrastructure.Messaging.Kafka.Serialization;

namespace UsersService.Infrastructure.Messaging.Kafka.Events.Keycloak
{
    public sealed class KeycloakAdminEventProcessor : IKeycloakAdminEventProcessor
    {
        private readonly IUserService _userService;
        private readonly ILogger<KeycloakAdminEventProcessor> _logger;

        public KeycloakAdminEventProcessor(
            ILogger<KeycloakAdminEventProcessor> logger,
            IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public async Task ProcessAsync(ConsumeResult<string, string> result, CancellationToken ct)
        {
            var adminEvent = JsonSerializer.Deserialize<KeycloakAdminEvent>(result.Message.Value, JsonOptions.Default);

            if (adminEvent is null)
            {
                throw new InvalidOperationException("Failed to deserialize Keycloak admin event.");
            }

            if (!string.Equals(adminEvent.ResourceType, "USER",StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Ignoring Keycloak admin event. ResourceType={ResourceType}, OperationType={OperationType}",
                    adminEvent.ResourceType, adminEvent.OperationType);

                return;
            }

            switch (adminEvent.OperationType)
            {
                case "UPDATE":
                    await ProcessUserUpdateAsync(adminEvent, ct);
                    break;
                case "DELETE":
                    await ProcessUserDeleteAsync(adminEvent, ct);
                    break;

                default:
                    _logger.LogDebug("Ignoring Keycloak admin USER event. OperationType={OperationType}", adminEvent.OperationType);
                    break;
            }
        }

        private async Task ProcessUserUpdateAsync(KeycloakAdminEvent adminEvent, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(adminEvent.Representation))
            {
                throw new InvalidOperationException("Keycloak USER UPDATE event does not contain representation.");
            }

            var user = JsonSerializer.Deserialize<KeycloakUserRepresentation>(adminEvent.Representation, JsonOptions.Default);

            if (user is null)
            {
                throw new InvalidOperationException("Failed to deserialize Keycloak user representation.");
            }

            if (string.IsNullOrWhiteSpace(user.Id))
            {
                throw new InvalidOperationException("Keycloak user representation does not contain id.");
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new InvalidOperationException("Keycloak user representation does not contain email.");
            }

            await _userService.UpdateEmailFromKeycloakAsync(
                new UpdateUserEmailFromKeycloakCommand(
                    user.Id,
                    user.Email),
                ct);

            _logger.LogInformation("Keycloak admin USER UPDATE processed. UserId={UserId}", user.Id);
        }

        private async Task ProcessUserDeleteAsync(KeycloakAdminEvent adminEvent, CancellationToken ct)
        {
            var userId = adminEvent.GetUserId();

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new InvalidOperationException("Keycloak USER DELETE event does not contain a valid userId.");
            }

            await _userService.DeleteFromKeycloakAsync(userId, ct);

            _logger.LogInformation("Keycloak admin USER DELETE processed. UserId={UserId}", userId);
        }
    }
}
