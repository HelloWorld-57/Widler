namespace UsersService.Application.Commands
{
    public record CreateUserCommand(
        string Name,
        string SecondName,
        string Email,
        DateTime BirthDate
    );
}
