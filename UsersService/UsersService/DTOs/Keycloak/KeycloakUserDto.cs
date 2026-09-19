namespace UsersService.DTOs.Keycloak
{
    public sealed record KeycloakUserDto(
        string Id,
        string Username,
        string? Email,
        bool Enabled);
}
