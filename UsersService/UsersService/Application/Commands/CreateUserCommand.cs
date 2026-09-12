namespace UsersService.Application.Commands
{
    public record CreateUserCommand(
        string Id,
        string Username,
        string Email
    );
}
