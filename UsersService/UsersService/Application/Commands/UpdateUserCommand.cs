namespace UsersService.Application.Commands
{
    public record UpdateUserCommand(
        string UserId,
        string? Name,
        string? SecondName,
        DateTime? BirthDate
    );
}
