namespace UsersService.Application.Commands
{
    public sealed record CreateUserFromKeycloakCommand(string UserId, string Username, string Email);
}
