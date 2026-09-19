using UsersService.DTOs.Keycloak;

namespace UsersService.Application.Interfaces
{
    public interface IKeycloakUserClient
    {
        Task<IReadOnlyCollection<KeycloakUserDto>> GetUsersAsync(CancellationToken ct);
    }
}
