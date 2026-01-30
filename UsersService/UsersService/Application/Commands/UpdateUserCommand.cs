namespace UsersService.Application.Commands
{
    public record UpdateUserCommand(
        string UserId,
        string? Name,
        string? SecondName,
        string? Email,
        DateTime? BirthDate
    );
}
