using UsersService.Application.Interfaces;
using UsersService.Domain.Entities;
using UsersService.DTOs;

namespace UsersService.Application.Services
{
    public sealed class UserReconciliationService : IUserReconciliationService
    {
        private readonly IUserRepository _repository;
        private readonly IKeycloakUserClient _keycloakClient;
        private readonly ILogger<UserReconciliationService> _logger;

        public UserReconciliationService(
            IUserRepository repository,
            IKeycloakUserClient keycloakClient,
            ILogger<UserReconciliationService> logger)
        {
            _repository = repository;
            _keycloakClient = keycloakClient;
            _logger = logger;
        }

        public async Task<ReconciliationResult> ReconcileAsync(CancellationToken ct)
        {
            _logger.LogInformation("Starting Keycloak users reconciliation");

            var keycloakUsers = await _keycloakClient.GetUsersAsync(ct);

            var localUsers = await _repository.GetAllIncludingDeletedAsync();

            var keycloakById = keycloakUsers.ToDictionary(x => x.Id);

            var localById = localUsers.ToDictionary(x => x.Id);

            var created = 0;
            var restored = 0;
            var updated = 0;
            var deleted = 0;

            foreach (var keycloakUser in keycloakUsers)
            {
                ct.ThrowIfCancellationRequested();

                if (!localById.TryGetValue(keycloakUser.Id, out var localUser))
                {
                    var user = new User(
                        keycloakUser.Id,
                        keycloakUser.Username,
                        keycloakUser.Email
                            ?? throw new InvalidOperationException($"Keycloak user '{keycloakUser.Id}' has no email."),
                        keycloakUser.Enabled);

                    await _repository.AddAsync(user);

                    created++;

                    continue;
                }

                if (localUser.IsDeleted)
                {
                    localUser.RestoreFromKeycloak(
                        keycloakUser.Username,
                        keycloakUser.Email!,
                        keycloakUser.Enabled);

                    restored++;

                    continue;
                }

                var before = (
                    localUser.Username,
                    localUser.Email,
                    localUser.IsEnabled);

                localUser.SynchronizeFromKeycloak(
                    keycloakUser.Username,
                    keycloakUser.Email,
                    keycloakUser.Enabled);

                var after = (
                    localUser.Username,
                    localUser.Email,
                    localUser.IsEnabled);

                if (before != after)
                {
                    updated++;
                }
            }

            foreach (var localUser in localUsers)
            {
                ct.ThrowIfCancellationRequested();

                if (localUser.IsDeleted)
                {
                    continue;
                }

                if (keycloakById.ContainsKey(localUser.Id))
                {
                    continue;
                }

                localUser.DeleteFromKeycloak();

                deleted++;
            }

            await _repository.SaveChangesAsync(ct);

            return new ReconciliationResult(
                KeycloakUsers: keycloakUsers.Count,
                LocalUsers: localUsers.Count,
                Created: created,
                Restored: restored,
                Updated: updated,
                Deleted: deleted);
        }
    }
}
