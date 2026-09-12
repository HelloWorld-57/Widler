namespace UsersService.Application.Commands
{
    public sealed record UpdateUserEmailFromKeycloakCommand(string UserId, string Email);
}
