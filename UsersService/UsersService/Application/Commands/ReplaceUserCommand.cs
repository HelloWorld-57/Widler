namespace UsersService.Application.Commands
{
    public record ReplaceUserCommand(
        string UserId,
        string Name,
        string SecondName,
        string Email,
        DateTime BirthDate);
}
