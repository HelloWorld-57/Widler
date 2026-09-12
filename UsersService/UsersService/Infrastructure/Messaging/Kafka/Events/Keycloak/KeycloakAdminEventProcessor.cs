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

            var keycloakUser = JsonSerializer.Deserialize<KeycloakUserRepresentation>(adminEvent.Representation, JsonOptions.Default);

            if (keycloakUser is null)
            {
                throw new InvalidOperationException("Failed to deserialize Keycloak user representation.");
            }

            if (string.IsNullOrWhiteSpace(keycloakUser.Id))
            {
                throw new InvalidOperationException("Keycloak user representation does not contain id.");
            }

            var user = await _userService.GetByIdAsync(keycloakUser.Id);

            if (user.IsEnabled != keycloakUser.Enabled)
            {
                if (keycloakUser.Enabled)
                {
                    await _userService.EnableFromKeycloakAsync(keycloakUser.Id, ct);
                }
                else
                {
                    await _userService.DisableFromKeycloakAsync(keycloakUser.Id, ct);
                }

                _logger.LogInformation(
                    "Keycloak user enabled state synchronized. UserId={UserId}, Enabled={Enabled}",
                    keycloakUser.Id,
                    keycloakUser.Enabled);

                //return;
            }

            //if (string.IsNullOrWhiteSpace(keycloakUser.Email))
            //{
            //    throw new InvalidOperationException("Keycloak user representation does not contain email.");
            //}

            if (!string.IsNullOrWhiteSpace(keycloakUser.Email) && 
                !String.Equals(user.Email, keycloakUser.Email, StringComparison.OrdinalIgnoreCase))
            {
                await _userService.UpdateEmailFromKeycloakAsync(
                new UpdateUserEmailFromKeycloakCommand(
                    keycloakUser.Id,
                    keycloakUser.Email),
                ct);
            }

            _logger.LogInformation("Keycloak admin USER UPDATE processed. UserId={UserId}", keycloakUser.Id);
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
